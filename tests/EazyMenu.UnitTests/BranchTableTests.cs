using System.Linq;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests;

public class BranchTableTests
{
    [Fact]
    public void AddTable_ShouldCreateTableWithUniqueLabel()
    {
        var branch = CreateBranch();

        var table = branch.AddTable("شماره 1", 4);

        Assert.Equal("شماره 1", table.Label);
        Assert.Single(branch.Tables);
    }

    [Fact]
    public void AddTable_WithDuplicateLabel_ShouldThrow()
    {
        var branch = CreateBranch();
        branch.AddTable("VIP", 4);

        Assert.Throws<DomainException>(() => branch.AddTable("vip", 6));
    }

    [Fact]
    public void UpdateTable_ShouldChangeProperties()
    {
        var branch = CreateBranch();
        var table = branch.AddTable("قدیمی", 4);

        branch.UpdateTable(table.Id, label: "جدید", capacity: 6, isOutdoor: true);

        var updated = branch.Tables.Single();
        Assert.Equal("جدید", updated.Label);
        Assert.Equal(6, updated.Capacity);
        Assert.True(updated.IsOutdoor);
    }

    [Fact]
    public void RemoveTable_ShouldDeleteFromBranch()
    {
        var branch = CreateBranch();
        var table = branch.AddTable("موقت", 2);

        branch.RemoveTable(table.Id);

        Assert.Empty(branch.Tables);
    }

    [Fact]
    public void MarkTableOutOfService_ShouldUpdateFlag()
    {
        var branch = CreateBranch();
        var table = branch.AddTable("کنار پنجره", 2);

        branch.MarkTableOutOfService(table.Id);

        Assert.True(branch.Tables.Single().IsOutOfService);
    }

    private static Branch CreateBranch()
    {
        var address = Address.Create("تهران", "خیابان 1", "1234567890");
        return Branch.Create("شعبه مرکزی", address);
    }
}
