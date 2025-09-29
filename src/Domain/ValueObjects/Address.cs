using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string City { get; }

    public string Street { get; }

    public string PostalCode { get; }

    private Address(string city, string street, string postalCode)
    {
        Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        Guard.AgainstNullOrWhiteSpace(street, nameof(street));
        Guard.AgainstNullOrWhiteSpace(postalCode, nameof(postalCode));

        City = city.Trim();
        Street = street.Trim();
        PostalCode = postalCode.Trim();
    }

    public static Address Create(string city, string street, string postalCode)
    {
        return new Address(city, street, postalCode);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return City.ToLowerInvariant();
        yield return Street.ToLowerInvariant();
        yield return PostalCode.ToLowerInvariant();
    }

    public override string ToString() => $"{Street}, {City} {PostalCode}";
}
