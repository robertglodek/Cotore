using Cotore.Serialization;

namespace Cotore.Requests;

internal sealed class PayloadValidator(ISchemaValidator schemaValidator, IJsonSerializer jsonSerializer) : IPayloadValidator
{
    public async Task<bool> TryValidate(ExecutionData executionData, HttpResponse httpResponse)
    {
        if (executionData.IsPayloadValid)
        {
            return true;
        }

        var response = new {errors = executionData.ValidationErrors};
        var payload = jsonSerializer.Serialize(response);
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

        return await schemaValidator.ValidateAsync(jsonSerializer.Serialize(payloadSchema.Payload), payloadSchema.Schema);
    }
}