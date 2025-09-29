using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests;

public class MoneyTests
{
    [Fact]
    public void From_WhenCreated_RoundsAmountToTwoDecimalPlaces()
    {
        var money = Money.From(1234.567m, "IRR");

        Assert.Equal(1234.57m, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void ToString_ReturnsFormattedCurrency()
    {
        var money = Money.From(500000m, "IRR");

        var formatted = money.ToString();

        Assert.Contains("IRR", formatted);
        Assert.Contains("500,000", formatted);
    }
}
