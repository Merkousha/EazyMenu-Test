using System;
using System.Collections.Generic;
using System.Text.Json;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(menu => menu.Id);

        builder.Property(menu => menu.Id)
            .HasConversion(id => id.Value, value => ToMenuId(value))
            .ValueGeneratedNever();

        builder.Property(menu => menu.TenantId)
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .IsRequired();

        var nameProperty = builder.Property(menu => menu.Name).IsRequired();
        ConfigureLocalizedTextProperty(nameProperty);

        var descriptionProperty = builder.Property<LocalizedText?>(menu => menu.Description);
        ConfigureNullableLocalizedTextProperty(descriptionProperty);

        builder.Property(menu => menu.IsDefault).IsRequired();
        builder.Property(menu => menu.IsArchived).IsRequired();
        builder.Property(menu => menu.CreatedAtUtc).IsRequired();
        builder.Property(menu => menu.UpdatedAtUtc).IsRequired();
        builder.Property(menu => menu.PublishedVersion).HasDefaultValue(0);

        builder.HasIndex(menu => new { menu.TenantId, menu.IsDefault })
            .HasDatabaseName("IX_Menus_Tenant_Default");

        builder.OwnsMany(menu => menu.Categories, categoryBuilder =>
        {
            categoryBuilder.ToTable("MenuCategories");
            categoryBuilder.WithOwner().HasForeignKey("MenuId");
            categoryBuilder.HasKey(category => category.Id);

            categoryBuilder.Property(category => category.Id)
                .HasConversion(id => id.Value, value => ToMenuCategoryId(value))
                .ValueGeneratedNever();

            var categoryNameProperty = categoryBuilder.Property(category => category.Name).IsRequired();
            ConfigureLocalizedTextProperty(categoryNameProperty);

            categoryBuilder.Property(category => category.DisplayOrder).IsRequired();
            categoryBuilder.Property(category => category.IconUrl).HasMaxLength(256);
            categoryBuilder.Property(category => category.IsArchived).IsRequired();

            categoryBuilder.HasIndex("MenuId", nameof(MenuCategory.DisplayOrder))
                .HasDatabaseName("IX_MenuCategories_DisplayOrder");

            categoryBuilder.OwnsMany(category => category.Items, itemBuilder =>
            {
                itemBuilder.ToTable("MenuItems");
                itemBuilder.WithOwner().HasForeignKey("CategoryId");
                itemBuilder.HasKey(item => item.Id);

                itemBuilder.Property(item => item.Id)
                    .HasConversion(id => id.Value, value => ToMenuItemId(value))
                    .ValueGeneratedNever();

                var itemNameProperty = itemBuilder.Property(item => item.Name).IsRequired();
                ConfigureLocalizedTextProperty(itemNameProperty);

                var itemDescriptionProperty = itemBuilder.Property<LocalizedText?>(item => item.Description);
                ConfigureNullableLocalizedTextProperty(itemDescriptionProperty);

                itemBuilder.Property(item => item.ImageUrl).HasMaxLength(512);
                itemBuilder.Property(item => item.DisplayOrder).IsRequired();
                itemBuilder.Property(item => item.IsAvailable).IsRequired();

                itemBuilder.OwnsOne(item => item.BasePrice, priceBuilder =>
                {
                    priceBuilder.Property(money => money.Amount)
                        .HasColumnName("BasePriceAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();

                    priceBuilder.Property(money => money.Currency)
                        .HasColumnName("BasePriceCurrency")
                        .HasMaxLength(8)
                        .IsRequired();
                });

                itemBuilder.Property<InventoryState>("Inventory")
                    .HasConversion(new InventoryStateValueConverter(), new InventoryStateValueComparer())
                    .HasColumnName("InventoryState")
                    .HasColumnType("nvarchar(64)")
                    .IsRequired();

                itemBuilder.Property<Dictionary<MenuChannel, Money>>("_channelPrices")
                    .HasConversion(new MenuChannelPriceValueConverter(), new MenuChannelPriceValueComparer())
                    .HasColumnName("ChannelPrices")
                    .HasColumnType("nvarchar(max)");

                itemBuilder.Property<HashSet<MenuTag>>("_tags")
                    .HasConversion(new MenuTagCollectionValueConverter(), new MenuTagCollectionValueComparer())
                    .HasColumnName("Tags")
                    .HasColumnType("nvarchar(256)");

                itemBuilder.HasIndex("CategoryId", nameof(MenuItem.DisplayOrder))
                    .HasDatabaseName("IX_MenuItems_DisplayOrder");
            });

            categoryBuilder.Navigation(category => category.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(menu => menu.Categories)
            .HasField("_categories")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureLocalizedTextProperty(PropertyBuilder<LocalizedText> propertyBuilder)
    {
        propertyBuilder.HasConversion(new LocalizedTextValueConverter());
        propertyBuilder.Metadata.SetValueComparer(new LocalizedTextValueComparer());
        propertyBuilder.HasColumnType("nvarchar(max)");
    }

    private static void ConfigureNullableLocalizedTextProperty(PropertyBuilder<LocalizedText?> propertyBuilder)
    {
        propertyBuilder.HasConversion(
            text => text == null ? null : JsonSerializer.Serialize(text.Values, JsonOptions),
            json => string.IsNullOrWhiteSpace(json)
                ? null
                : LocalizedText.FromDictionary(JsonSerializer.Deserialize<Dictionary<string, string>>(json!, JsonOptions) ?? new Dictionary<string, string>()));

        propertyBuilder.Metadata.SetValueComparer(new NullableLocalizedTextValueComparer());
        propertyBuilder.HasColumnType("nvarchar(max)");
    }

    private static MenuId ToMenuId(Guid value)
    {
        if (!MenuId.TryCreate(value, out var id))
        {
            throw new InvalidOperationException("شناسه منو ذخیره‌شده نامعتبر است.");
        }

        return id;
    }

    private static TenantId ToTenantId(Guid value)
    {
        if (!TenantId.TryCreate(value, out var id))
        {
            throw new InvalidOperationException("شناسه مستاجر ذخیره‌شده نامعتبر است.");
        }

        return id;
    }

    private static MenuCategoryId ToMenuCategoryId(Guid value)
    {
        if (!MenuCategoryId.TryCreate(value, out var id))
        {
            throw new InvalidOperationException("شناسه دسته منو ذخیره‌شده نامعتبر است.");
        }

        return id;
    }

    private static MenuItemId ToMenuItemId(Guid value)
    {
        if (!MenuItemId.TryCreate(value, out var id))
        {
            throw new InvalidOperationException("شناسه آیتم منو ذخیره‌شده نامعتبر است.");
        }

        return id;
    }
}
