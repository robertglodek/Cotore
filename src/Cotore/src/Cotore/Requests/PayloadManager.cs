using Newtonsoft.Json;
using Cotore.Options;

namespace Cotore.Requests;

internal sealed class PayloadManager : IPayloadManager
{
    private readonly CotoreOptions _options;
    public IDictionary<string, PayloadSchema> Payloads { get; }

    public PayloadManager(IOptions<CotoreOptions> options)
    {
        _options = options.Value;
        Payloads = LoadPayloads();
    }

    private Dictionary<string, PayloadSchema> LoadPayloads()
    {
        if (_options.Modules is null)
        {
            return [];
        }
        
        var payloads = new Dictionary<string, PayloadSchema>();
        var modulesPath = _options.ModulesPath;
        modulesPath = string.IsNullOrWhiteSpace(modulesPath)
            ? string.Empty
            : (modulesPath.EndsWith('/') ? modulesPath : $"{modulesPath}/");

        foreach (var module in _options.Modules)
        {
            foreach (var route in module.Value.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.Payload))
                {
                    continue;
                }

                var payloadsFolder = _options.PayloadsFolder;
                var fullPath = $"{modulesPath}{module.Value.Name}/{payloadsFolder}/{route.Payload}";
                var fullJsonPath = fullPath.EndsWith(".json") ? fullPath : $"{fullPath}.json";
                if (!File.Exists(fullJsonPath))
                {
                    continue;
                }

                var schemaPath = $"{modulesPath}{module.Value.Name}/{payloadsFolder}/{route.Schema}";
                var fullSchemaPath = schemaPath.EndsWith(".json") ? schemaPath : $"{schemaPath}.json";
                var schema = string.Empty;
                if (File.Exists(fullSchemaPath))
                {
                    schema = File.ReadAllText(fullSchemaPath);
                }

                var json = File.ReadAllText(fullJsonPath);
                dynamic expandoObject = new ExpandoObject();
                JsonConvert.PopulateObject(json, expandoObject);
                var upstream = string.IsNullOrWhiteSpace(route.Upstream) ? string.Empty : route.Upstream;
                if (!string.IsNullOrWhiteSpace(module.Value.Path))
                {
                    var modulePath = module.Value.Path.EndsWith('/') ? module.Value.Path : $"{module.Value.Path}/";
                    if (upstream.StartsWith('/'))
                    {
                        upstream = upstream[1..];
                    }

                    if (upstream.EndsWith('/'))
                    {
                        upstream = upstream[..^1];
                    }

                    upstream = $"{modulePath}{upstream}";
                }

                if (string.IsNullOrWhiteSpace(upstream))
                {
                    upstream = "/";
                }

                payloads.Add(GetKey(route.Method, upstream), new PayloadSchema(expandoObject, schema));
            }
        }

        return payloads;
    }

    public string GetKey(string? method, string upstream) => $"{method?.ToLowerInvariant()}:{upstream}";
}