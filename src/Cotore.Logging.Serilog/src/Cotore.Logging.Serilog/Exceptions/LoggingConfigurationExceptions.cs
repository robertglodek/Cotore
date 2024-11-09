using Cotore.Exceptions;

namespace Cotore.Logging.Serilog.Exceptions;

public sealed class LoggingConfigurationExceptions(string message) : CustomException(message);