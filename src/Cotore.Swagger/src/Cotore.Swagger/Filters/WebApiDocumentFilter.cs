using Cotore.Serialization;
using Cotore.WebApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cotore.Swagger.Filters;

internal sealed class WebApiDocumentFilter(IJsonSerializer jsonSerializer, WebApiEndpointDefinitions definitions) : IDocumentFilter
{
    private readonly IJsonSerializer _jsonSerializer = jsonSerializer;
    private readonly WebApiEndpointDefinitions _definitions = definitions;
    private const string InBody = "body";
    private const string InQuery = "query";
    private const string InPath = "path";
    private const string InHeader = "header";
    private const string BodyContentKey = "body";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var definition in _definitions)
        {
            var pathItem = new OpenApiPathItem();
            var (operation, type) = GetOperation(pathItem, definition.Method);
            if (operation is null) continue;

            operation.Responses = [];
            operation.Parameters = [];

            AddParameters(operation, definition.Parameters);
            AddResponses(operation, definition.Responses);

            var path = definition.Path ?? "/";
            if (swaggerDoc.Paths.TryGetValue(path, out var existingPathItem))
            {
                existingPathItem.AddOperation((OperationType)type!, operation);
            }
            else
            {
                swaggerDoc.Paths.Add(path, pathItem);
            }
        }
    }

    private static (OpenApiOperation? operation, OperationType? type) GetOperation(OpenApiPathItem pathItem, string method)
    {
        if (!Enum.TryParse<OperationType>(method, true, out var operationType)) return (null, null);
        pathItem.AddOperation(operationType, new OpenApiOperation());
        return (pathItem.Operations[operationType], operationType);
    }

    private void AddParameters(OpenApiOperation operation, IEnumerable<WebApiEndpointParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.In == InBody)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [System.Net.Mime.MediaTypeNames.Application.Json] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = parameter.Type,
                                Example = new OpenApiString(_jsonSerializer.Serialize(parameter.Example))
                            }
                        }
                    }
                };
            }
            else
            {
                var exampleValue = parameter.Example is string || parameter.Example is ValueType
                    ? new OpenApiString(parameter.Example.ToString())
                    : new OpenApiString(_jsonSerializer.Serialize(parameter.Example));

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = parameter.Name,
                    In = parameter.In switch
                    {
                        InQuery => ParameterLocation.Query,
                        InPath => ParameterLocation.Path,
                        InHeader => ParameterLocation.Header,
                        _ => ParameterLocation.Query
                    },
                    Schema = new OpenApiSchema
                    {
                        Type = parameter.Type,
                        Example = exampleValue
                    }
                });
            }
        }
    }

    private void AddResponses(OpenApiOperation operation, IEnumerable<WebApiEndpointResponse> responses)
    {
        foreach (var response in responses)
        {
            operation.Responses.Add(response.StatusCode.ToString(), new OpenApiResponse
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [BodyContentKey] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = response.Type,
                            Example = new OpenApiString(_jsonSerializer.Serialize(response.Example))
                        }
                    }
                }
            });
        }
    }
}