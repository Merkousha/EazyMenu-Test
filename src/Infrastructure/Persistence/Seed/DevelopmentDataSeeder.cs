using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EazyMenu.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeder برای ایجاد داده‌های نمونه برای تست و توسعه
/// </summary>
public sealed class DevelopmentDataSeeder
{
    private readonly EazyMenuDbContext _context;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(EazyMenuDbContext context, ILogger<DevelopmentDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("شروع Seed کردن داده‌های نمونه...");

        try
        {
            // بررسی اینکه آیا داده قبلی وجود دارد
            if (await _context.Tenants.AnyAsync())
            {
                _logger.LogInformation("داده‌های نمونه قبلاً ایجاد شده‌اند. Seed متوقف شد.");
                return;
            }

            // 1. ایجاد Tenant (رستوران)
            var tenant = await SeedTenantAsync();
            
            // 2. ایجاد کاربر مدیر
            var ownerUser = await SeedOwnerUserAsync(tenant.Id);
            
            // 3. ایجاد منوی اول رستوران
            var menu = await SeedMenuAsync(tenant.Id);

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seed داده‌های نمونه با موفقیت انجام شد!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطا در Seed کردن داده‌های نمونه");
            throw;
        }
    }

    private async Task<Tenant> SeedTenantAsync()
    {
        _logger.LogInformation("در حال ایجاد Tenant نمونه...");

        var contactPhone = PhoneNumber.Create("+982188776655");
        var address = Address.Create(
            city: "تهران",
            street: "خیابان ولیعصر، نرسیده به پارک ملت",
            postalCode: "1234567890"
        );

        var contactEmail = Email.Create("info@gilanrestaurant.ir");
        var branding = BrandProfile.Create(
            displayName: "رستوران سنتی گیلان",
            logoUrl: "https://via.placeholder.com/200x200?text=Gilan+Logo",
            primaryColor: "#2C5F2D",
            secondaryColor: "#97BC62",
            templateName: "modern-template"
        );

        // اضافه کردن سایر اطلاعات برند
        branding = branding.Update(
            bannerImageUrl: "https://via.placeholder.com/1200x400?text=Gilan+Banner",
            aboutText: "رستوران سنتی گیلان با بیش از ۲۰ سال سابقه، ارائه‌دهنده بهترین غذاهای سنتی شمالی است.",
            openingHours: "روزهای هفته: ۱۲:۰۰ - ۲۳:۰۰"
        );
        branding = branding.Publish();

        var tenant = Tenant.Register(
            businessName: "رستوران سنتی گیلان",
            branding: branding,
            contactEmail: contactEmail,
            contactPhone: contactPhone,
            headquartersAddress: address
        );

        // ذخیره Tenant بدون Subscription (برای اجتناب از Circular Dependency)
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // ایجاد و فعال‌سازی اشتراک
        var subscriptionStart = DateTime.UtcNow;
        var subscriptionEnd = subscriptionStart.AddMonths(3);
        var subscriptionPrice = EazyMenu.Domain.ValueObjects.Money.From(5000000, "IRR"); // 5 میلیون تومان
        var subscription = Subscription.Create(
            plan: SubscriptionPlan.Pro,
            price: subscriptionPrice,
            startDateUtc: subscriptionStart,
            endDateUtc: subscriptionEnd,
            isTrial: false
        );
        
        tenant.RegisterPendingSubscription(subscription);
        tenant.ActivateSubscription(subscription);

        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Tenant '{TenantName}' با slug '{Slug}' ایجاد شد", tenant.BusinessName, tenant.Slug.Value);
        return tenant;
    }

    private async Task<User> SeedOwnerUserAsync(TenantId tenantId)
    {
        _logger.LogInformation("در حال ایجاد کاربر مدیر...");

        var phoneNumber = PhoneNumber.Create("+989123456789");
        
        // هش کردن رمز عبور: Admin@123
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12);
        var passwordHashObj = PasswordHash.Create(passwordHash, "bcrypt");

        var user = User.Create(
            tenantId: tenantId,
            email: "admin@restaurant.test",
            fullName: "احمد محمدی",
            phoneNumber: phoneNumber,
            passwordHash: passwordHashObj,
            role: UserRole.Owner
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ کاربر مدیر '{Email}' ایجاد شد", user.Email);
        return user;
    }

    private async Task<Menu> SeedMenuAsync(TenantId tenantId)
    {
        _logger.LogInformation("در حال ایجاد منوی نمونه...");

        // ایجاد منوی اصلی
        var menu = Menu.Create(
            tenantId: tenantId,
            name: LocalizedText.Create("منوی اصلی"),
            description: LocalizedText.Create("منوی کامل رستوران با انواع غذاهای سنتی و مدرن"),
            isDefault: true
        );

        // دسته‌بندی 1: پیش‌غذاها
        var appetizerCategory = menu.AddCategory(
            name: LocalizedText.Create("پیش‌غذاها"),
            iconUrl: null,
            displayOrder: 1
        );

        menu.AddItem(
            categoryId: appetizerCategory.Id,
            name: LocalizedText.Create("میرزا قاسمی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(180000, "IRR"),
            description: LocalizedText.Create("بادمجان کبابی با سیر و تخم مرغ"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: appetizerCategory.Id,
            name: LocalizedText.Create("کوکو سبزی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(150000, "IRR"),
            description: LocalizedText.Create("کوکوی سنتی با سبزیجات معطر"),
            isAvailable: true
        );

        // دسته‌بندی 2: غذاهای اصلی
        var mainCourseCategory = menu.AddCategory(
            name: LocalizedText.Create("غذاهای اصلی"),
            iconUrl: null,
            displayOrder: 2
        );

        menu.AddItem(
            categoryId: mainCourseCategory.Id,
            name: LocalizedText.Create("کباب کوبیده"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(450000, "IRR"),
            description: LocalizedText.Create("دو سیخ کباب کوبیده با برنج و گوجه"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: mainCourseCategory.Id,
            name: LocalizedText.Create("جوجه کباب"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(420000, "IRR"),
            description: LocalizedText.Create("جوجه کباب زعفرانی با برنج"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: mainCourseCategory.Id,
            name: LocalizedText.Create("قورمه سبزی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(380000, "IRR"),
            description: LocalizedText.Create("خورش سنتی با لوبیا قرمز و سبزیجات"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: mainCourseCategory.Id,
            name: LocalizedText.Create("قیمه بادمجان"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(360000, "IRR"),
            description: LocalizedText.Create("خورش قیمه با بادمجان سرخ شده"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: mainCourseCategory.Id,
            name: LocalizedText.Create("زرشک پلو با مرغ"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(400000, "IRR"),
            description: LocalizedText.Create("برنج زعفرانی با زرشک و مرغ"),
            isAvailable: true
        );

        // دسته‌بندی 3: نوشیدنی‌ها
        var beverageCategory = menu.AddCategory(
            name: LocalizedText.Create("نوشیدنی‌ها"),
            iconUrl: null,
            displayOrder: 3
        );

        menu.AddItem(
            categoryId: beverageCategory.Id,
            name: LocalizedText.Create("دوغ سنتی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(35000, "IRR"),
            description: LocalizedText.Create("دوغ خانگی با نعنا"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: beverageCategory.Id,
            name: LocalizedText.Create("آب معدنی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(25000, "IRR"),
            description: null,
            isAvailable: true
        );

        menu.AddItem(
            categoryId: beverageCategory.Id,
            name: LocalizedText.Create("چای سنتی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(30000, "IRR"),
            description: LocalizedText.Create("چای تازه دم با هل"),
            isAvailable: true
        );

        // دسته‌بندی 4: دسرها
        var dessertCategory = menu.AddCategory(
            name: LocalizedText.Create("دسرها"),
            iconUrl: null,
            displayOrder: 4
        );

        menu.AddItem(
            categoryId: dessertCategory.Id,
            name: LocalizedText.Create("بستنی سنتی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(120000, "IRR"),
            description: LocalizedText.Create("بستنی زعفرانی با پسته"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: dessertCategory.Id,
            name: LocalizedText.Create("فالوده شیرازی"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(110000, "IRR"),
            description: LocalizedText.Create("فالوده با آب لیمو و یخ"),
            isAvailable: true
        );

        menu.AddItem(
            categoryId: dessertCategory.Id,
            name: LocalizedText.Create("شله زرد"),
            basePrice: EazyMenu.Domain.ValueObjects.Money.From(90000, "IRR"),
            description: LocalizedText.Create("دسر سنتی با زعفران و دارچین"),
            isAvailable: true
        );

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ منو با {CategoryCount} دسته‌بندی ایجاد شد", 4);
        return menu;
    }
}
