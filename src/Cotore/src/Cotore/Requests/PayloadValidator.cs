using Cotore.Serialization;
using Microsoft.AspNetCore.Http;

namespace Cotore.Requests;

internal sealed class PayloadValidator(ISchemaValidator schemaValidator, IJsonSerializer jsonSerializer) : IPayloadValidator
{
    private readonly ISchemaValidator _schemaValidator = schemaValidator;
    private readonly IJsonSerializer _jsonSerializer = jsonSerializer;

    public async Task<bool> TryValidate(ExecutionData executionData, HttpResponse httpResponse)
    {
        if (executionData.IsPayloadValid)
        {
            return true;
        }

        var response = new {errors = executionData.ValidationErrors};
        var payload = _jsonSerializer.Serialize(response);
        httpResponse.ContentType = "application/json";
        await httpResponse.WriteAsync(payload);

        return false;
    }
    
    public async Task<IEnumerable<Error>> GetValidationErrorsAsync(PayloadSchema payloadSchema)
    {
        if (string.IsNullOrWhiteSpace(payloadSchema.Schema))
        {
            return [];
        }

        return await _schemaValidator.ValidateAsync(_jsonSerializer.Serialize(payloadSchema.Payload), payloadSchema.Schema);
    }
}