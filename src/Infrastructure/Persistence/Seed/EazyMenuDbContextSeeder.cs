using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Seed;

public static class EazyMenuDbContextSeeder
{
    public static async Task SeedAsync(EazyMenuDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (await context.Menus.AnyAsync(cancellationToken))
        {
            return;
        }

        var tenant = Tenant.Register(
            "کافه نمونه ایزی‌منو",
            BrandProfile.Create("EasyMenu Café", logoUrl: "https://cdn.eazymenu.ir/assets/logo-sample.svg", primaryColor: "#FF5722"),
            Email.Create("owner@eazymenu.ir"),
            PhoneNumber.Create("+989121234567"),
            Address.Create("تهران", "خیابان ولیعصر", "1234567890"));

        tenant.AddBranch(
            "شعبه مرکزی",
            Address.Create("تهران", "میدان ونک، خیابان گاندی", "02188123456"));

        var menu = Menu.Create(
            tenant.Id,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "منوی اصلی",
                ["en-US"] = "Main Menu"
            }),
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "نمونه‌ای از تنظیمات چندزبانه برای پیش‌نمایش UI",
                ["en-US"] = "Sample bilingual menu for the UI preview"
            }));

        menu.MarkAsDefault();

        var mainsCategory = menu.AddCategory(LocalizedText.FromDictionary(new Dictionary<string, string>
        {
            [LocalizedText.DefaultCulture] = "غذاهای اصلی",
            ["en-US"] = "Main Courses"
        }));

        var drinksCategory = menu.AddCategory(LocalizedText.FromDictionary(new Dictionary<string, string>
        {
            [LocalizedText.DefaultCulture] = "نوشیدنی‌ها",
            ["en-US"] = "Beverages"
        }));

        menu.AddItem(
            mainsCategory.Id,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "قرمه‌سبزی",
                ["en-US"] = "Ghormeh Sabzi"
            }),
            Money.From(350_000),
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "خورش سبزیجات تازه با گوشت گوسفندی و لوبیا قرمز",
                ["en-US"] = "Herb stew with lamb and red beans"
            }),
            isAvailable: true,
            inventory: InventoryState.Track(24, 6),
            imageUrl: "https://cdn.eazymenu.ir/assets/menu/ghormeh-sabzi.jpg",
            channelPrices: new Dictionary<MenuChannel, Money>
            {
                [MenuChannel.Delivery] = Money.From(360_000),
                [MenuChannel.TakeAway] = Money.From(355_000)
            },
            tags: new[] { MenuTag.Popular, MenuTag.ChefSpecial });

        menu.AddItem(
            mainsCategory.Id,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "چلوکباب سلطانی",
                ["en-US"] = "Sultani Kebab"
            }),
            Money.From(420_000),
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "ترکیب فیله و کوبیده با برنج ایرانی",
                ["en-US"] = "Combination of fillet and koobideh with saffron rice"
            }),
            isAvailable: true,
            inventory: InventoryState.Track(18, 5),
            imageUrl: "https://cdn.eazymenu.ir/assets/menu/sultani-kebab.jpg",
            channelPrices: new Dictionary<MenuChannel, Money>
            {
                [MenuChannel.Delivery] = Money.From(430_000)
            },
            tags: new[] { MenuTag.ChefSpecial });

        menu.AddItem(
            drinksCategory.Id,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "شربت زعفران",
                ["en-US"] = "Saffron Sharbat"
            }),
            Money.From(85_000),
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "نوشیدنی خنک با زعفران و گلاب",
                ["en-US"] = "Chilled saffron and rosewater drink"
            }),
            isAvailable: true,
            inventory: InventoryState.Infinite(),
            imageUrl: "https://cdn.eazymenu.ir/assets/menu/saffron-drink.jpg",
            channelPrices: new Dictionary<MenuChannel, Money>
            {
                [MenuChannel.Delivery] = Money.From(90_000)
            },
            tags: new[] { MenuTag.New, MenuTag.Popular });

        menu.AddItem(
            drinksCategory.Id,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "چای ایرانی",
                ["en-US"] = "Persian Tea"
            }),
            Money.From(40_000),
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "چای سیلان دم‌آوری شده با هل",
                ["en-US"] = "Ceylon tea brewed with cardamom"
            }),
            isAvailable: true,
            inventory: InventoryState.Infinite(),
            imageUrl: "https://cdn.eazymenu.ir/assets/menu/persian-tea.jpg",
            channelPrices: new Dictionary<MenuChannel, Money>
            {
                [MenuChannel.TakeAway] = Money.From(45_000)
            },
            tags: new[] { MenuTag.Popular });

        menu.PublishNextVersion();

        context.Tenants.Add(tenant);
        context.Menus.Add(menu);

        await context.SaveChangesAsync(cancellationToken);
    }
}
