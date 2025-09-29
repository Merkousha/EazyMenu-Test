using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class QrCodeReference : ValueObject
{
    public string Value { get; }

    private QrCodeReference(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        var normalized = value.Trim();
        if (normalized.Length < 4)
        {
            throw new DomainException("شناسه QR باید حداقل ۴ کاراکتر باشد.");
        }

        Value = normalized;
    }

    public static QrCodeReference Create(string value) => new(value);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
