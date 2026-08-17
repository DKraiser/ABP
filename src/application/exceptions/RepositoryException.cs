using System.Runtime.Serialization;

namespace ABP.Application.Exceptions;

public class RepositoryException : InvalidOperationException
{
    public RepositoryException()
    {
    }

    public RepositoryException(string? message) : base(message)
    {
    }

    public RepositoryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}