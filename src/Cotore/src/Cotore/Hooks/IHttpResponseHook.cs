namespace Cotore.Hooks;

public interface IHttpResponseHook
{
    Task InvokeAsync(HttpResponseMessage response, ExecutionData data);
}