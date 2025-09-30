using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Menus;

public sealed class MenuItem : Entity<MenuItemId>
{
    private readonly Dictionary<MenuChannel, Money> _channelPrices = new();
    private readonly HashSet<MenuTag> _tags = new();

    private MenuItem(MenuItemId id, LocalizedText name, Money basePrice, LocalizedText? description, InventoryState inventory, string? imageUrl, bool isAvailable, int displayOrder, IDictionary<MenuChannel, Money>? channelPrices, IEnumerable<MenuTag>? tags)
        : base(id)
    {
        Guard.AgainstNull(name, nameof(name));
        Guard.AgainstNull(basePrice, nameof(basePrice));
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));
        GuardAgainstInvalidImage(imageUrl);

        Name = name;
        Description = description;
        BasePrice = basePrice;
    Inventory = inventory ?? InventoryState.Infinite();
    ImageUrl = SanitizeImageUrl(imageUrl);
    IsAvailable = isAvailable && Inventory.IsAvailable;
        DisplayOrder = displayOrder;

        if (channelPrices is not null)
        {
            foreach (var pair in channelPrices)
            {
                _channelPrices[pair.Key] = pair.Value;
            }
        }

        if (tags is not null)
        {
            foreach (var tag in tags.Distinct())
            {
                _tags.Add(tag);
            }
        }
    }

    private MenuItem()
    {
    }

    public LocalizedText Name { get; private set; } = null!;

    public LocalizedText? Description { get; private set; }

    public Money BasePrice { get; private set; } = null!;

    public bool IsAvailable { get; private set; }

    public InventoryState Inventory { get; private set; } = InventoryState.Infinite();

    public string? ImageUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public IReadOnlyDictionary<MenuChannel, Money> ChannelPrices => new ReadOnlyDictionary<MenuChannel, Money>(_channelPrices);

    public IReadOnlyCollection<MenuTag> Tags => new ReadOnlyCollection<MenuTag>(_tags.OrderBy(tag => tag).ToList());

    internal static MenuItem Create(LocalizedText name, Money basePrice, int displayOrder, LocalizedText? description = null, bool isAvailable = true, InventoryState? inventory = null, string? imageUrl = null, IDictionary<MenuChannel, Money>? channelPrices = null, IEnumerable<MenuTag>? tags = null)
    {
        return new MenuItem(MenuItemId.New(), name, basePrice, description, inventory ?? InventoryState.Infinite(), imageUrl, isAvailable, displayOrder, channelPrices, tags);
    }

    internal void UpdateDetails(LocalizedText name, LocalizedText? description, string? imageUrl)
    {
        Guard.AgainstNull(name, nameof(name));
        GuardAgainstInvalidImage(imageUrl);

        Name = name;
        Description = description;
        ImageUrl = SanitizeImageUrl(imageUrl);
    }

    internal void UpdatePricing(Money basePrice, IDictionary<MenuChannel, Money>? channelPrices)
    {
        Guard.AgainstNull(basePrice, nameof(basePrice));

        BasePrice = basePrice;
        _channelPrices.Clear();

        if (channelPrices is null)
        {
            return;
        }

        foreach (var pair in channelPrices)
        {
            _channelPrices[pair.Key] = pair.Value;
        }
    }

    internal void SetAvailability(bool isAvailable)
    {
        if (!isAvailable)
        {
            IsAvailable = false;
            return;
        }

        IsAvailable = Inventory.IsAvailable;
    }

    internal void SetInventory(InventoryState inventory)
    {
        Guard.AgainstNull(inventory, nameof(inventory));
        Inventory = inventory;
        if (!Inventory.IsAvailable)
        {
            IsAvailable = false;
        }
    }

    internal void AdjustInventory(int delta)
    {
        if (delta == 0 || Inventory.Mode == InventoryTrackingMode.Infinite)
        {
            return;
        }

        Inventory = delta > 0 ? Inventory.Increase(delta) : Inventory.Decrease(Math.Abs(delta));

        if (!Inventory.IsAvailable)
        {
            IsAvailable = false;
        }
        else if (!IsAvailable)
        {
            IsAvailable = true;
        }
    }

    internal void ReplaceTags(IEnumerable<MenuTag>? tags)
    {
        _tags.Clear();
        if (tags is null)
        {
            return;
        }

        foreach (var tag in tags.Distinct())
        {
            _tags.Add(tag);
        }
    }

    internal void SetDisplayOrder(int displayOrder)
    {
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));
        DisplayOrder = displayOrder;
    }

    internal void RemoveChannelPrice(MenuChannel channel)
    {
        _channelPrices.Remove(channel);
    }

    private static void GuardAgainstInvalidImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var trimmed = imageUrl.Trim();
        if (trimmed.Length > 512)
        {
            throw new DomainException("آدرس تصویر نباید بیش از ۵۱۲ کاراکتر باشد.");
        }

        if (!Uri.TryCreate(trimmed, UriKind.RelativeOrAbsolute, out _))
        {
            throw new DomainException("آدرس تصویر معتبر نیست.");
        }
    }

    private static string? SanitizeImageUrl(string? imageUrl)
    {
        return string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }
}
