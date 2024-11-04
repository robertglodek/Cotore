namespace Cotore.Requests;

internal interface ISchemaValidator
{
    Task<IEnumerable<Error>> ValidateAsync(string payload, string schema);
}