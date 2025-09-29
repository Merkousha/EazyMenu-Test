using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex Pattern = new("^[+]?\u200c?[0-9]{8,15}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        var normalized = value.Trim().Replace("\u200c", string.Empty);
        if (!Pattern.IsMatch(normalized))
        {
            throw new DomainException("شماره تلفن وارد شده معتبر نیست.");
        }

        Value = normalized;
    }

    public static PhoneNumber Create(string value) => new(value);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
