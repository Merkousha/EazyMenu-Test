using System;

namespace EazyMenu.Domain.Common.Exceptions;

/// <summary>
/// استثنای نقض قوانین تجاری دامنه.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
