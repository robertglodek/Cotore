using NJsonSchema;

namespace Cotore.Requests;

internal sealed class SchemaValidator : ISchemaValidator
{
    public async Task<IEnumerable<Error>> ValidateAsync(string payload, string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            return [];
        }

        var jsonSchema = await JsonSchema.FromJsonAsync(schema);
        var errors = jsonSchema.Validate(payload);

        return errors.Select(e => new Error
        {
            Code = e.Kind.ToString(),
            Property = e.Property,
            Message = e.ToString()
        });
    }
}