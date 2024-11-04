namespace Cotore.Logging.Serilog;

public class LoggingService: ILoggingService
{
    public void SetLoggingLevel(string logEventLevel)
        => Extensions.LoggingLevelSwitch.MinimumLevel = Extensions.GetLogEventLevel(logEventLevel);
}
