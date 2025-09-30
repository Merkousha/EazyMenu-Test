using System.Linq;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Domain.Menus;

public class MenuTests
{
    [Fact]
    public void Create_ShouldPopulateFieldsAndRaiseEvent()
    {
        var tenantId = TenantId.New();

        var menu = Menu.Create(tenantId, LocalizedText.Create("منوی اصلی"));

        Assert.Equal(tenantId, menu.TenantId);
        Assert.Equal("منوی اصلی", menu.Name.GetValue(LocalizedText.DefaultCulture));
        Assert.False(menu.IsArchived);

        var domainEvent = Assert.IsType<MenuCreatedDomainEvent>(menu.DomainEvents.Single());
        Assert.Equal(menu.Id, domainEvent.MenuId);
        Assert.Equal(tenantId, domainEvent.TenantId);
    }

    [Fact]
    public void AddCategory_ShouldAssignSequentialDisplayOrder()
    {
        var menu = Menu.Create(TenantId.New(), LocalizedText.Create("منوی روز"));
        menu.ClearDomainEvents();

        var breakfast = menu.AddCategory(LocalizedText.Create("صبحانه"));
        var lunch = menu.AddCategory(LocalizedText.Create("ناهار"));

        Assert.Equal(0, breakfast.DisplayOrder);
        Assert.Equal(1, lunch.DisplayOrder);
    }

    [Fact]
    public void ReorderCategories_ShouldUpdateDisplayOrderAndRaiseEvent()
    {
        var menu = Menu.Create(TenantId.New(), LocalizedText.Create("منوی اصلی"));
        menu.ClearDomainEvents();

        var breakfast = menu.AddCategory(LocalizedText.Create("صبحانه"));
        var lunch = menu.AddCategory(LocalizedText.Create("ناهار"));

        menu.ClearDomainEvents();
        menu.ReorderCategories(new[] { lunch.Id, breakfast.Id });

        Assert.Equal(0, lunch.DisplayOrder);
        Assert.Equal(1, breakfast.DisplayOrder);

        var @event = Assert.IsType<MenuCategoriesReorderedDomainEvent>(menu.DomainEvents.Single());
        Assert.Equal(new[] { lunch.Id, breakfast.Id }, @event.OrderedCategoryIds);
    }

    [Fact]
    public void UpdateMenuItemPricing_ShouldRaisePriceChangedEvent()
    {
        var menu = Menu.Create(TenantId.New(), LocalizedText.Create("منوی اصلی"));
        menu.ClearDomainEvents();
        var category = menu.AddCategory(LocalizedText.Create("غذاهای اصلی"));
        var item = menu.AddItem(category.Id, LocalizedText.Create("چلوکباب"), Money.From(250_000m));

        menu.ClearDomainEvents();
        menu.UpdateMenuItemPricing(category.Id, item.Id, Money.From(275_000m), null);

        var @event = Assert.IsType<MenuItemPriceChangedDomainEvent>(menu.DomainEvents.Single());
        Assert.Equal(menu.Id, @event.MenuId);
        Assert.Equal(item.Id, @event.MenuItemId);
        Assert.Equal(275_000m, @event.NewPrice.Amount);
    }

    [Fact]
    public void AdjustMenuItemInventory_ToZero_ShouldDisableItemAndRaiseAvailabilityEvent()
    {
        var menu = Menu.Create(TenantId.New(), LocalizedText.Create("منوی اصلی"));
        menu.ClearDomainEvents();
        var category = menu.AddCategory(LocalizedText.Create("پیش‌غذا"));
        var item = menu.AddItem(
            category.Id,
            LocalizedText.Create("سوپ"),
            Money.From(90_000m),
            inventory: InventoryState.Track(5));

        menu.ClearDomainEvents();
        menu.AdjustMenuItemInventory(category.Id, item.Id, -5);

        Assert.False(item.IsAvailable);

        var @event = Assert.IsType<MenuItemAvailabilityChangedDomainEvent>(menu.DomainEvents.Single());
        Assert.Equal(item.Id, @event.MenuItemId);
        Assert.False(@event.IsAvailable);
    }
}
