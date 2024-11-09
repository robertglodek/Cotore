using System.Net.Http.Headers;
using System.Text;
using Cotore.Hooks;
using Cotore.Options;
using System.Diagnostics;
using Cotore.Serialization;
using Cotore.Requests;
using System.Text.Json;

namespace Cotore.Handlers;

internal sealed class DownstreamHandler(IServiceProvider serviceProvider, IRequestProcessor requestProcessor,
    IPayloadValidator payloadValidator, IOptions<CotoreOptions> options, IHttpClientFactory httpClientFactory,
    ILogger<DownstreamHandler> logger, IJsonSerializer jsonSerializer) : IHandler
{
    private const string ContentTypeApplicationJson = "application/json";
    private const string ContentTypeHeader = "Content-Type";
    private static readonly string[] ExcludedResponseHeaders = ["transfer-encoding", "content-length"];
    private static readonly HttpContent EmptyContent = new StringContent("{}", Encoding.UTF8, ContentTypeApplicationJson);
    private readonly CotoreOptions _options = options.Value;
    private readonly IEnumerable<IRequestHook> _requestHooks = serviceProvider.GetServices<IRequestHook>();
    private readonly IEnumerable<IResponseHook> _responseHooks = serviceProvider.GetServices<IResponseHook>();
    private readonly IEnumerable<IHttpRequestHook> _httpRequestHooks = serviceProvider.GetServices<IHttpRequestHook>();
    private readonly IEnumerable<IHttpResponseHook> _httpResponseHooks = serviceProvider.GetServices<IHttpResponseHook>();

    public async Task HandleAsync(HttpContext context, RouteConfig config, CancellationToken cancellationToken = default)
    {
        var activity = Activity.Current;

        if (config.Route.Downstream is null)
        {
            return;
        }

        var executionData = await requestProcessor.ProcessAsync(config, context);

        foreach (var hook in _requestHooks)
        {
            await hook.InvokeAsync(context.Request, executionData, cancellationToken);
        }

        if (!executionData.IsPayloadValid)
        {
            await payloadValidator.TryValidate(executionData, context.Response);
            return;
        }

        if (string.IsNullOrWhiteSpace(executionData.Downstream))
        {
            return;
        }

        logger.LogInformation("Sending HTTP {RequestMethod} request to: {DownstreamUrl} [Activity ID: {ActivityId}]",
            context.Request.Method, config.Downstream, activity!.Id);

        var response = await SendRequestAsync(executionData);
        if (response is null)
        {
            logger.LogWarning("Did not receive HTTP response for: {DownstreamUrl}", executionData.Route.Downstream);
            return;
        }

        await WriteResponseAsync(context.Response, response, executionData);
    }

    private async Task<HttpResponseMessage?> SendRequestAsync(ExecutionData executionData)
    {
        var httpClient = httpClientFactory.CreateClient("Cotore");
        var method = (string.IsNullOrWhiteSpace(executionData.Route.DownstreamMethod)
            ? executionData.Context.Request.Method
            : executionData.Route.DownstreamMethod).ToLowerInvariant();

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(executionData.Downstream)
        };

        if (executionData.Route.ForwardRequestHeaders == true ||
            _options.ForwardRequestHeaders == true && executionData.Route.ForwardRequestHeaders != false)
        {
            foreach (var (key, value) in executionData.Context.Request.Headers)
            {
                request.Headers.TryAddWithoutValidation(key, value.ToArray());
            }
        }

        var requestHeaders = executionData.Route.RequestHeaders is null ||
                             executionData.Route.RequestHeaders.Count == 0
            ? _options.RequestHeaders
            : executionData.Route.RequestHeaders;

        if (requestHeaders is { })
        {
            foreach (var (key, value) in requestHeaders)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                    continue;
                }

                if (!executionData.Context.Request.Headers.TryGetValue(key, out var values))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(key, values.ToArray());
            }
        }

        var includeBody = false;
        switch (method)
        {
            case "get":
                request.Method = HttpMethod.Get;
                break;
            case "post":
                request.Method = HttpMethod.Post;
                includeBody = true;
                break;
            case "put":
                request.Method = HttpMethod.Put;
                includeBody = true;
                break;
            case "patch":
                request.Method = HttpMethod.Patch;
                includeBody = true;
                break;
            case "delete":
                request.Method = HttpMethod.Delete;
                break;
            case "head":
                request.Method = HttpMethod.Head;
                break;
            case "options":
                request.Method = HttpMethod.Options;
                break;
            case "trace":
                request.Method = HttpMethod.Trace;
                break;
            default:
                return null;
        }

        foreach (var hook in _httpRequestHooks)
        {
            await hook.InvokeAsync(request, executionData);
        }

        if (!includeBody)
        {
            return await httpClient.SendAsync(request);
        }

        using var content = GetHttpContent(executionData);
        request.Content = content;
        return await httpClient.SendAsync(request);
    }

    private HttpContent GetHttpContent(ExecutionData executionData)
    {
        var data = executionData.Payload;
        var contentType = executionData.ContentType;
        if (executionData.HasPayload)
        {
            if (data is null || !contentType.StartsWith(ContentTypeApplicationJson))
            {
                return EmptyContent;
            }

            return new StringContent(jsonSerializer.Serialize(data), Encoding.UTF8, ContentTypeApplicationJson);
        }

        if (executionData.Context.Request.Body is null)
        {
            return EmptyContent;
        }

        var httpContent = new StreamContent(executionData.Context.Request.Body);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return httpContent;
    }

    private async Task WriteResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
        ExecutionData executionData)
    {
        var method = executionData.Context.Request.Method;
        var activity = Activity.Current;

        if (activity != null)
        {
            response.Headers.Append("traceparent", $"00-{activity.TraceId}-{activity.SpanId}-01");
        }

        foreach (var hook in _httpResponseHooks)
        {
            await hook.InvokeAsync(httpResponse, executionData);
        }

        foreach (var hook in _responseHooks)
        {
            await hook.InvokeAsync(response, executionData);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogInformation("Received an invalid response ({StatusCode}) to HTTP {Method} request from: {DownstreamUrl} [Activity ID: {ActivityId}]",
                                   httpResponse.StatusCode, method, executionData.Route.Downstream, activity!.Id);
            await SetErrorResponseAsync(response, httpResponse, executionData);
            return;
        }

        logger.LogInformation("Received a successful response ({StatusCode}) to HTTP {Method} request from: {DownstreamUrl} [Activity ID: {ActivityId}]",
                               httpResponse.StatusCode, method, executionData.Route.Downstream, activity!.Id);

        await SetSuccessResponseAsync(response, httpResponse, executionData);
    }

    private static async Task SetErrorResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
        ExecutionData executionData)
    {
        var onError = executionData.Route.OnError;
        var content = await httpResponse.Content.ReadAsStringAsync();
        if (executionData.Context.Request.Method is "GET" && !response.Headers.ContainsKey(ContentTypeHeader))
        {
            response.Headers[ContentTypeHeader] = ContentTypeApplicationJson;
        }

        if (onError is null)
        {
            response.StatusCode = (int)httpResponse.StatusCode;
            await response.WriteAsync(content);
            return;
        }

        response.StatusCode = onError.Code > 0 ? onError.Code : 400;
        await response.WriteAsync(content);
    }

    private async Task SetSuccessResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
        ExecutionData executionData)
    {
        const string responseDataKey = "response.data";
        var content = await httpResponse.Content.ReadAsStringAsync();
        var onSuccess = executionData.Route.OnSuccess;
        if (_options.ForwardStatusCode == false || executionData.Route.ForwardStatusCode == false)
        {
            response.StatusCode = 200;
        }
        else
        {
            response.StatusCode = (int)httpResponse.StatusCode;
        }

        if (executionData.Route.ForwardResponseHeaders == true ||
            (_options.ForwardResponseHeaders == true && executionData.Route.ForwardResponseHeaders != false))
        {
            foreach (var header in httpResponse.Headers)
            {
                if (ExcludedResponseHeaders.Contains(header.Key.ToLowerInvariant()))
                {
                    continue;
                }

                if (response.Headers.ContainsKey(header.Key))
                {
                    continue;
                }

                response.Headers.Append(header.Key, header.Value.ToArray());
            }

            // Fixed for missing Content-Type header
            foreach (var header in httpResponse.Content.Headers)
            {
                if (ExcludedResponseHeaders.Contains(header.Key.ToLowerInvariant()))
                {
                    continue;
                }

                if (response.Headers.ContainsKey(header.Key))
                {
                    continue;
                }

                response.Headers.Append(header.Key, header.Value.ToArray());
            }
        }

        var responseHeaders = executionData.Route.ResponseHeaders is null ||
                              executionData.Route.ResponseHeaders.Count == 0
            ? _options.ResponseHeaders ?? []
            : executionData.Route.ResponseHeaders;

        foreach (var header in responseHeaders)
        {
            if (string.IsNullOrWhiteSpace(header.Value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(header.Value))
            {
                response.Headers.Remove(header.Key);
                response.Headers.Append(header.Key, header.Value);
                continue;
            }

            if (!httpResponse.Headers.TryGetValues(header.Key, out var values))
            {
                continue;
            }

            response.Headers.Remove(header.Key);
            response.Headers.Append(header.Key, values.ToArray());
        }

        if (executionData.Context.Request.Method is "GET" && !response.Headers.ContainsKey(ContentTypeHeader))
        {
            response.Headers[ContentTypeHeader] = ContentTypeApplicationJson;
        }

        if (onSuccess is null)
        {
            if (response.StatusCode != 204)
            {
                await response.WriteAsync(content);
            }

            return;
        }

        response.StatusCode = onSuccess.Code > 0 ? onSuccess.Code : response.StatusCode;
        if (response.StatusCode == 204)
        {
            return;
        }

        if (onSuccess.Data is string dataText && dataText.StartsWith(responseDataKey))
        {
            var dataKey = dataText.Replace(responseDataKey, string.Empty);
            if (string.IsNullOrWhiteSpace(dataKey))
            {
                await response.WriteAsync(content);
                return;
            }

            dataKey = dataKey[1..];
            using var jsonDoc = JsonDocument.Parse(content);
            var rootElement = jsonDoc.RootElement;

            if (rootElement.TryGetProperty(dataKey, out var dataValue))
            {
                switch (dataValue.ValueKind)
                {
                    case JsonValueKind.Object:
                        await response.WriteAsync(dataValue.ToString());
                        return;
                    case JsonValueKind.Array:
                        await response.WriteAsync(dataValue.ToString());
                        return;
                    default:
                        await response.WriteAsync(dataValue.ToString());
                        break;
                }
            }
            return;
        }

        if (onSuccess.Data is not null)
        {
            await response.WriteAsync(onSuccess.Data.ToString() ?? string.Empty);
        }
    }
}