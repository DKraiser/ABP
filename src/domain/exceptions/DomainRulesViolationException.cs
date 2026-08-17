using System.Runtime.Serialization;

namespace ABP.Domain.Exceptions;

/// <summary>
/// Should be thrown if domain rules are violated.
/// </summary>
public class DomainRulesViolationException : Exception
{
    public DomainRulesViolationException()
    {
    }

    public DomainRulesViolationException(string? message) : base(message)
    {
    }

    public DomainRulesViolationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DomainRulesViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}