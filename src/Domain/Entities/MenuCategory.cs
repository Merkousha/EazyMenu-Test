using System;
using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Entities;

public class MenuCategory
{
    private readonly List<MenuItem> _items = new();

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }
    public IReadOnlyCollection<MenuItem> Items => _items;

    public MenuCategory(string name, int displayOrder = 0)
    {
        Name = name;
        DisplayOrder = displayOrder;
    }

    public void Update(string name, int displayOrder)
    {
        Name = name;
        DisplayOrder = displayOrder;
    }

    public void AddItem(MenuItem item)
    {
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        _items.RemoveAll(i => i.Id == itemId);
    }
}
