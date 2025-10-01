using System;

namespace EazyMenu.Domain.Common.Exceptions;

/// <summary>
/// استثنای اعتبارسنجی دامنه که هنگام نقض قوانین اعتبارسنجی پرتاب می‌شود.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message)
        : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
