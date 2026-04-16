using Pixario.Ingest.Application.Exceptions;

namespace Pixario.Ingest.Infrastructure.Exceptions;

public class ExternalServiceBadRequestException(string message) : PermanentProcessingException(message);