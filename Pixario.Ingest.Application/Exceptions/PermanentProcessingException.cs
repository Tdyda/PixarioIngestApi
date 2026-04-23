namespace Pixario.Ingest.Application.Exceptions;

public class PermanentProcessingException : Exception
{
    public PermanentProcessingException(string message) : base(message)
    {
    }

    public PermanentProcessingException(string message, Exception inner) : base(message, inner)
    {
    }
}