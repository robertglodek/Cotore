namespace Cotore.Hooks;

public interface IRequestHook
{
    Task InvokeAsync(HttpRequest request, ExecutionData data);
}