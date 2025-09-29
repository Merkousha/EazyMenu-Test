using System;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Money Price { get; private set; }
    public bool IsAvailable { get; private set; }

    public MenuItem(string name, Money price, string? description = null, bool isAvailable = true)
    {
        Name = name;
        Price = price;
        Description = description;
        IsAvailable = isAvailable;
    }

    public void UpdateDetails(string name, Money price, string? description)
    {
        Name = name;
        Price = price;
        Description = description;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }
}
