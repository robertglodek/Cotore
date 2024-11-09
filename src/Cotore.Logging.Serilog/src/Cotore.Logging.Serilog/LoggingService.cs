namespace Cotore.Logging.Serilog;

public sealed class LoggingService : ILoggingService
{
    public void SetLoggingLevel(string logEventLevel)
        => Extensions.LoggingLevelSwitch.MinimumLevel = Extensions.GetLogEventLevel(logEventLevel);
}
