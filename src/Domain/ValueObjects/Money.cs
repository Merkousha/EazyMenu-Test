using System;
using System.Globalization;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public static Money From(decimal amount, string currency = "IRR")
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required", nameof(currency));
        }

        return new Money(amount, currency);
    }

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} {1:N0}", Currency, Amount);
}
