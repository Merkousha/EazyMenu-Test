using System;
using System.Collections.Generic;
using System.Globalization;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public const string DefaultCurrency = "IRR";

    public decimal Amount { get; }

    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Guard.AgainstNegative(amount, nameof(amount));
        Guard.AgainstNullOrWhiteSpace(currency, nameof(currency));

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public static Money From(decimal amount, string currency = DefaultCurrency)
    {
        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new DomainException("مبلغ نمی‌تواند منفی شود.");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        Guard.AgainstNegative(factor, nameof(factor));
        return new Money(Amount * factor, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsZero() => Amount == 0;

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} {1:N0}", Currency, Amount);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    private void EnsureSameCurrency(Money other)
    {
        Guard.AgainstNull(other, nameof(other));

        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("ارزهای مبلغ با هم هم‌خوانی ندارند.");
        }
    }
}
