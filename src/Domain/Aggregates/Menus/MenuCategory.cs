using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Menus;

public sealed class MenuCategory : Entity<MenuCategoryId>
{
    private readonly List<MenuItem> _items = new();

    private MenuCategory(MenuCategoryId id, LocalizedText name, int displayOrder, string? iconUrl, bool isArchived)
        : base(id)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));
        GuardAgainstInvalidIcon(iconUrl);

        Name = name;
        DisplayOrder = displayOrder;
        IconUrl = SanitizeIcon(iconUrl);
        IsArchived = isArchived;
    }

    private MenuCategory()
    {
    }

    public LocalizedText Name { get; private set; } = null!;

    public int DisplayOrder { get; private set; }

    public string? IconUrl { get; private set; }

    public bool IsArchived { get; private set; }

    public IReadOnlyCollection<MenuItem> Items => new ReadOnlyCollection<MenuItem>(_items.OrderBy(item => item.DisplayOrder).ToList());

    internal static MenuCategory Create(LocalizedText name, int displayOrder, string? iconUrl = null)
    {
        return new MenuCategory(MenuCategoryId.New(), name, displayOrder, iconUrl, false);
    }

    internal void Update(LocalizedText name, int displayOrder, string? iconUrl)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));
        GuardAgainstInvalidIcon(iconUrl);

        EnsureActive();

        Name = name;
        DisplayOrder = displayOrder;
        IconUrl = SanitizeIcon(iconUrl);
    }

    internal MenuItem AddItem(LocalizedText name, Money basePrice, LocalizedText? description, bool isAvailable, InventoryState? inventory, string? imageUrl, IDictionary<MenuChannel, Money>? channelPrices, IEnumerable<MenuTag>? tags)
    {
        EnsureActive();

        var displayOrder = _items.Count == 0 ? 0 : _items.Max(item => item.DisplayOrder) + 1;
        var item = MenuItem.Create(name, basePrice, displayOrder, description, isAvailable, inventory, imageUrl, channelPrices, tags);
        _items.Add(item);
        return item;
    }

    internal void RemoveItem(MenuItemId itemId)
    {
        EnsureActive();

        var item = _items.SingleOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new DomainException("آیتم مورد نظر در این دسته یافت نشد.");
        }

        _items.Remove(item);
    }

    internal MenuItem GetItem(MenuItemId itemId)
    {
        var item = _items.SingleOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new DomainException("آیتم مورد نظر در این دسته یافت نشد.");
        }

        return item;
    }

    internal void ReorderItems(IReadOnlyList<MenuItemId> orderedIds)
    {
        EnsureActive();

        if (orderedIds.Count != _items.Count)
        {
            throw new DomainException("تعداد آیتم‌ها با لیست مرتب‌سازی هم‌خوانی ندارد.");
        }

        var lookup = _items.ToDictionary(item => item.Id);

        for (var index = 0; index < orderedIds.Count; index++)
        {
            var id = orderedIds[index];
            if (!lookup.TryGetValue(id, out var item))
            {
                throw new DomainException("آیتمی با شناسه ارائه‌شده یافت نشد.");
            }

            item.SetDisplayOrder(index);
        }

        _items.Sort((left, right) => left.DisplayOrder.CompareTo(right.DisplayOrder));
    }

    internal void Archive()
    {
        IsArchived = true;
    }

    internal void Restore()
    {
        IsArchived = false;
    }

    internal void SetDisplayOrder(int displayOrder)
    {
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));
        DisplayOrder = displayOrder;
    }

    private void EnsureActive()
    {
        if (IsArchived)
        {
            throw new DomainException("این دسته آرشیو شده است و قابل ویرایش نیست.");
        }
    }

    private static void GuardAgainstInvalidIcon(string? iconUrl)
    {
        if (string.IsNullOrWhiteSpace(iconUrl))
        {
            return;
        }

        var trimmed = iconUrl.Trim();
        if (trimmed.Length > 256)
        {
            throw new DomainException("آدرس آیکن نباید بیش از ۲۵۶ کاراکتر باشد.");
        }
    }

    private static string? SanitizeIcon(string? iconUrl)
    {
        return string.IsNullOrWhiteSpace(iconUrl) ? null : iconUrl.Trim();
    }
}
