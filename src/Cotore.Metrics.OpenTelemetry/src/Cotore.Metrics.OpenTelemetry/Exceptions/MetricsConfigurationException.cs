using Cotore.Exceptions;

namespace Cotore.Metrics.OpenTelemetry.Exceptions;

public sealed class MetricsConfigurationException(string message) : CustomException(message);