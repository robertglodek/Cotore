namespace Cotore.Hooks;

public interface IResponseHook
{
    Task InvokeAsync(HttpResponse response, ExecutionData data);
}