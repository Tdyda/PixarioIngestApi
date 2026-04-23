using Pixario.Ingest.Application.Exceptions;

namespace Pixario.Ingest.Infrastructure.Exceptions;

public class ExternalServiceUnavailableException(string message) : TemporaryProcessingException(message);