using Cotore.Exceptions;

namespace Cotore.Swagger.Exceptions;

public sealed class SwaggerConfigurationException(string message) : CustomException(message);