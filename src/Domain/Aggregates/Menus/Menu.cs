using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Menus;

public sealed class Menu : Entity<MenuId>, IAggregateRoot
{
    private readonly List<MenuCategory> _categories = new();

    private Menu(MenuId id, TenantId tenantId, LocalizedText name, LocalizedText? description, bool isDefault)
        : base(id)
    {
        Guard.AgainstDefault(tenantId, nameof(tenantId));
        Guard.AgainstNull(name, nameof(name));

        TenantId = tenantId;
        Name = name;
        Description = description;
        IsDefault = isDefault;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    private Menu()
    {
    }

    public TenantId TenantId { get; private set; }

    public LocalizedText Name { get; private set; } = null!;

    public LocalizedText? Description { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public int PublishedVersion { get; private set; }

    public IReadOnlyCollection<MenuCategory> Categories => new ReadOnlyCollection<MenuCategory>(_categories.OrderBy(category => category.DisplayOrder).ToList());

    public static Menu Create(TenantId tenantId, LocalizedText name, LocalizedText? description = null, bool isDefault = false)
    {
        var menu = new Menu(MenuId.New(), tenantId, name, description, isDefault);
        menu.RaiseDomainEvent(new MenuCreatedDomainEvent(menu.Id, tenantId));
        return menu;
    }

    public void Rename(LocalizedText name)
    {
        Guard.AgainstNull(name, nameof(name));
        Name = name;
        Touch();
    }

    public void UpdateDescription(LocalizedText? description)
    {
        Description = description;
        Touch();
    }

    public void MarkAsDefault()
    {
        if (IsDefault)
        {
            return;
        }

        IsDefault = true;
        Touch();
    }

    public void UnsetDefault()
    {
        if (!IsDefault)
        {
            return;
        }

        IsDefault = false;
        Touch();
    }

    public void Archive()
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        Touch();
    }

    public void Restore()
    {
        if (!IsArchived)
        {
            return;
        }

        IsArchived = false;
        Touch();
    }

    public MenuCategory AddCategory(LocalizedText name, string? iconUrl = null, int? displayOrder = null)
    {
        Guard.AgainstNull(name, nameof(name));
        EnsureActive();

        var order = displayOrder ?? NextCategoryOrder();
        if (_categories.Any(category => category.DisplayOrder == order))
        {
            throw new DomainException("ترتیب نمایش دسته تکراری است.");
        }

        var category = MenuCategory.Create(name, order, iconUrl);
        _categories.Add(category);
        ReorderInternal();
        Touch();
        return category;
    }

    public void UpdateCategory(MenuCategoryId categoryId, LocalizedText name, int displayOrder, string? iconUrl)
    {
        Guard.AgainstNull(name, nameof(name));
        EnsureActive();

        var category = FindCategory(categoryId);
        if (_categories.Any(other => other.Id != categoryId && other.DisplayOrder == displayOrder))
        {
            throw new DomainException("ترتیب نمایش دسته تکراری است.");
        }

        category.Update(name, displayOrder, iconUrl);
        ReorderInternal();
        Touch();
    }

    public void ArchiveCategory(MenuCategoryId categoryId)
    {
        var category = FindCategory(categoryId);
        category.Archive();
        Touch();
    }

    public void RestoreCategory(MenuCategoryId categoryId)
    {
        var category = FindCategory(categoryId);
        category.Restore();
        Touch();
    }

    public void RemoveCategory(MenuCategoryId categoryId)
    {
        var category = _categories.SingleOrDefault(c => c.Id == categoryId);
        if (category is null)
        {
            throw new DomainException("دسته مورد نظر یافت نشد.");
        }

        _categories.Remove(category);
        ReorderInternal();
        Touch();
    }

    public MenuItem AddItem(MenuCategoryId categoryId, LocalizedText name, Money basePrice, LocalizedText? description = null, bool isAvailable = true, InventoryState? inventory = null, string? imageUrl = null, IDictionary<MenuChannel, Money>? channelPrices = null, IEnumerable<MenuTag>? tags = null)
    {
        EnsureActive();
        var category = FindCategory(categoryId);
        var item = category.AddItem(name, basePrice, description, isAvailable, inventory, imageUrl, channelPrices, tags);
        Touch();
        return item;
    }

    public void RemoveMenuItem(MenuCategoryId categoryId, MenuItemId itemId)
    {
        EnsureActive();
        var category = FindCategory(categoryId);
        category.RemoveItem(itemId);
        Touch();
    }

    public void UpdateMenuItemDetails(MenuCategoryId categoryId, MenuItemId itemId, LocalizedText name, LocalizedText? description, string? imageUrl)
    {
        Guard.AgainstNull(name, nameof(name));
        EnsureActive();

        var item = FindItem(categoryId, itemId);
        item.UpdateDetails(name, description, imageUrl);
        Touch();
    }

    public void UpdateMenuItemTags(MenuCategoryId categoryId, MenuItemId itemId, IEnumerable<MenuTag>? tags)
    {
        EnsureActive();

        var item = FindItem(categoryId, itemId);
        item.ReplaceTags(tags);
        Touch();
    }

    public void UpdateMenuItemPricing(MenuCategoryId categoryId, MenuItemId itemId, Money basePrice, IDictionary<MenuChannel, Money>? channelPrices)
    {
        Guard.AgainstNull(basePrice, nameof(basePrice));
        EnsureActive();

        var item = FindItem(categoryId, itemId);
        var previousBasePrice = item.BasePrice;
        item.UpdatePricing(basePrice, channelPrices);
        Touch();

        if (!previousBasePrice.Equals(basePrice))
        {
            RaiseDomainEvent(new MenuItemPriceChangedDomainEvent(Id, itemId, basePrice));
        }
    }

    public void SetMenuItemAvailability(MenuCategoryId categoryId, MenuItemId itemId, bool isAvailable)
    {
        EnsureActive();

        var item = FindItem(categoryId, itemId);
        if (item.IsAvailable == isAvailable)
        {
            return;
        }

        item.SetAvailability(isAvailable);
        Touch();
        RaiseDomainEvent(new MenuItemAvailabilityChangedDomainEvent(Id, itemId, item.IsAvailable));
    }

    public void AdjustMenuItemInventory(MenuCategoryId categoryId, MenuItemId itemId, int delta)
    {
        EnsureActive();

        var item = FindItem(categoryId, itemId);
        var wasAvailable = item.IsAvailable;
        item.AdjustInventory(delta);
        Touch();

        if (item.IsAvailable != wasAvailable)
        {
            RaiseDomainEvent(new MenuItemAvailabilityChangedDomainEvent(Id, itemId, item.IsAvailable));
        }
    }

    public void ReorderCategories(IReadOnlyList<MenuCategoryId> orderedIds)
    {
        EnsureActive();

        if (orderedIds.Count != _categories.Count)
        {
            throw new DomainException("تعداد دسته‌ها با لیست مرتب‌سازی هم‌خوانی ندارد.");
        }

        var lookup = _categories.ToDictionary(category => category.Id);

        for (var index = 0; index < orderedIds.Count; index++)
        {
            var id = orderedIds[index];
            if (!lookup.TryGetValue(id, out var category))
            {
                throw new DomainException("دسته‌ای با شناسه ارائه‌شده یافت نشد.");
            }

            category.SetDisplayOrder(index);
        }

        ReorderInternal();
        Touch();
        RaiseDomainEvent(new MenuCategoriesReorderedDomainEvent(Id, orderedIds.ToArray()));
    }

    public void ReorderMenuItems(MenuCategoryId categoryId, IReadOnlyList<MenuItemId> orderedIds)
    {
        EnsureActive();
        var category = FindCategory(categoryId);
        category.ReorderItems(orderedIds);
        Touch();
        RaiseDomainEvent(new MenuItemsReorderedDomainEvent(Id, categoryId, orderedIds.ToArray()));
    }

    public void PublishNextVersion()
    {
        PublishedVersion++;
        Touch();
        RaiseDomainEvent(new MenuPublishedDomainEvent(Id, TenantId, PublishedVersion));
    }

    private MenuCategory FindCategory(MenuCategoryId categoryId)
    {
        var category = _categories.SingleOrDefault(c => c.Id == categoryId);
        if (category is null)
        {
            throw new DomainException("دسته مورد نظر یافت نشد.");
        }

        return category;
    }

    private MenuItem FindItem(MenuCategoryId categoryId, MenuItemId itemId)
    {
        var category = FindCategory(categoryId);
        return category.GetItem(itemId);
    }

    private void EnsureActive()
    {
        if (IsArchived)
        {
            throw new DomainException("این منو آرشیو شده است و قابل ویرایش نیست.");
        }
    }

    private int NextCategoryOrder()
    {
        return _categories.Count == 0 ? 0 : _categories.Max(category => category.DisplayOrder) + 1;
    }

    private void ReorderInternal()
    {
        _categories.Sort((left, right) => left.DisplayOrder.CompareTo(right.DisplayOrder));
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
