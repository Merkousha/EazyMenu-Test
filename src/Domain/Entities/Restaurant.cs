using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Entities;

public class Restaurant : IAggregateRoot
{
    private readonly List<MenuCategory> _categories = new();

    public TenantId TenantId { get; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? LogoUrl { get; private set; }
    public IReadOnlyCollection<MenuCategory> Categories => _categories;

    public Restaurant(TenantId tenantId, string name, string slug)
    {
        TenantId = tenantId;
        Name = name;
        Slug = slug;
    }

    public void UpdateBranding(string name, string? logoUrl)
    {
        Name = name;
        LogoUrl = logoUrl;
    }

    public void RegisterCategory(MenuCategory category)
    {
        if (_categories.All(c => c.Id != category.Id))
        {
            _categories.Add(category);
        }
    }
}
