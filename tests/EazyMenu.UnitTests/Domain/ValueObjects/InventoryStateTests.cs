using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Domain.ValueObjects;

public class InventoryStateTests
{
    [Fact]
    public void Track_DecreaseBelowZero_ShouldThrow()
    {
        var inventory = InventoryState.Track(2);

        Assert.Throws<System.InvalidOperationException>(() => inventory.Decrease(3));
    }

    [Fact]
    public void Increase_ShouldAdjustQuantity()
    {
        var inventory = InventoryState.Track(1);
        var updated = inventory.Increase(4);

        Assert.Equal(5, updated.Quantity);
        Assert.True(updated.IsAvailable);
    }

    [Fact]
    public void Decrease_ToZero_ShouldMarkUnavailable()
    {
        var inventory = InventoryState.Track(3);
        var updated = inventory.Decrease(3);

        Assert.Equal(0, updated.Quantity);
        Assert.False(updated.IsAvailable);
    }
}
