using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Web.Models.Menus;

public sealed class MenuLocalizedTextInput
{
    [Display(Name = "نام فارسی")]
    [Required(ErrorMessage = "وارد کردن نام فارسی الزامی است.")]
    [MaxLength(64, ErrorMessage = "نام فارسی نمی‌تواند بیش از ۶۴ کاراکتر باشد.")]
    public string Fa { get; set; } = string.Empty;

    [Display(Name = "نام انگلیسی")]
    [MaxLength(64, ErrorMessage = "نام انگلیسی نمی‌تواند بیش از ۶۴ کاراکتر باشد.")]
    public string? En { get; set; }

    [Display(Name = "نام عربی")]
    [MaxLength(64, ErrorMessage = "نام عربی نمی‌تواند بیش از ۶۴ کاراکتر باشد.")]
    public string? Ar { get; set; }

    public IDictionary<string, string> ToDictionary()
    {
        var dictionary = new Dictionary<string, string>
        {
            [LocalizedText.DefaultCulture] = Fa.Trim()
        };

        if (!string.IsNullOrWhiteSpace(En))
        {
            dictionary["en-US"] = En.Trim();
        }

        if (!string.IsNullOrWhiteSpace(Ar))
        {
            dictionary["ar-SA"] = Ar.Trim();
        }

        return dictionary;
    }
}

public sealed class MenuDescriptionInput
{
    [Display(Name = "توضیح فارسی")]
    [MaxLength(512, ErrorMessage = "توضیح فارسی نمی‌تواند بیش از ۵۱۲ کاراکتر باشد.")]
    public string? Fa { get; set; }

    [Display(Name = "توضیح انگلیسی")]
    [MaxLength(512, ErrorMessage = "توضیح انگلیسی نمی‌تواند بیش از ۵۱۲ کاراکتر باشد.")]
    public string? En { get; set; }

    [Display(Name = "توضیح عربی")]
    [MaxLength(512, ErrorMessage = "توضیح عربی نمی‌تواند بیش از ۵۱۲ کاراکتر باشد.")]
    public string? Ar { get; set; }

    public IDictionary<string, string>? ToDictionary()
    {
        var dictionary = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(Fa))
        {
            dictionary[LocalizedText.DefaultCulture] = Fa.Trim();
        }

        if (!string.IsNullOrWhiteSpace(En))
        {
            dictionary["en-US"] = En.Trim();
        }

        if (!string.IsNullOrWhiteSpace(Ar))
        {
            dictionary["ar-SA"] = Ar.Trim();
        }

        return dictionary.Count == 0 ? null : dictionary;
    }
}

public sealed class InventoryInput
{
    [Display(Name = "نوع موجودی")]
    public string Mode { get; set; } = "Infinite";

    [Display(Name = "تعداد موجودی")]
    public int? Quantity { get; set; }

    [Display(Name = "آستانه هشدار")]
    public int? Threshold { get; set; }

    public InventoryPayload? ToPayload()
    {
        if (string.Equals(Mode, "Track", StringComparison.OrdinalIgnoreCase))
        {
            if (Quantity is null)
            {
                throw new ValidationException("برای پیگیری موجودی، مقدار موجودی فعلی الزامی است.");
            }

            var quantity = Math.Max(0, Quantity.Value);
            var threshold = Threshold.HasValue ? Math.Max(0, Threshold.Value) : (int?)null;
            return new InventoryPayload("Track", quantity, threshold);
        }

        return new InventoryPayload("Infinite", null, null);
    }
}

public sealed class CreateMenuCategoryInput
{
    [Required]
    public MenuLocalizedTextInput Name { get; set; } = new();

    [Display(Name = "آیکون (URL)")]
    [Url(ErrorMessage = "فرمت آدرس آیکون معتبر نیست.")]
    public string? IconUrl { get; set; }

    public int? DisplayOrder { get; set; }
}

public sealed class UpdateMenuCategoryInput
{
    [Required]
    public MenuLocalizedTextInput Name { get; set; } = new();

    [Display(Name = "آیکون (URL)")]
    [Url(ErrorMessage = "فرمت آدرس آیکون معتبر نیست.")]
    public string? IconUrl { get; set; }

    [Display(Name = "ترتیب نمایش")]
    [Range(0, int.MaxValue, ErrorMessage = "ترتیب نمایش نمی‌تواند منفی باشد.")]
    public int DisplayOrder { get; set; }
}

public sealed class ReorderCategoriesInput
{
    [Required]
    public IReadOnlyList<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
}

public sealed class CreateMenuItemInput
{
    [Required]
    public MenuLocalizedTextInput Name { get; set; } = new();

    public MenuDescriptionInput? Description { get; set; }

    [Display(Name = "قیمت پایه")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت پایه نمی‌تواند منفی باشد.")]
    public decimal BasePrice { get; set; }

    [Display(Name = "واحد پول")]
    public string Currency { get; set; } = "IRT";

    [Display(Name = "فعال است؟")]
    public bool IsAvailable { get; set; } = true;

    public InventoryInput? Inventory { get; set; }

    [Display(Name = "تصویر (URL)")]
    [Url(ErrorMessage = "آدرس تصویر معتبر نیست.")]
    public string? ImageUrl { get; set; }

    [Display(Name = "قیمت‌های کانال")]
    public ChannelPriceInput ChannelPrices { get; set; } = new();

    [Display(Name = "برچسب‌ها")]
    public string? Tags { get; set; }

    public IReadOnlyCollection<string>? ParseTags()
    {
        if (string.IsNullOrWhiteSpace(Tags))
        {
            return null;
        }

        var values = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value.Replace(" ", string.Empty, StringComparison.Ordinal));
        }

        return result.Count == 0 ? null : result;
    }
}

public sealed class UpdateMenuItemInput
{
    [Required]
    public MenuLocalizedTextInput Name { get; set; } = new();

    public MenuDescriptionInput? Description { get; set; }

    [Display(Name = "تصویر (URL)")]
    [Url(ErrorMessage = "آدرس تصویر معتبر نیست.")]
    public string? ImageUrl { get; set; }

    [Display(Name = "برچسب‌ها")]
    public string? Tags { get; set; }

    [Display(Name = "قیمت پایه")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت پایه نمی‌تواند منفی باشد.")]
    public decimal BasePrice { get; set; }

    [Display(Name = "واحد پول")]
    public string Currency { get; set; } = "IRT";

    public ChannelPriceInput ChannelPrices { get; set; } = new();

    [Display(Name = "فعال است؟")]
    public bool IsAvailable { get; set; } = true;

    public IReadOnlyCollection<string>? ParseTags()
    {
        if (string.IsNullOrWhiteSpace(Tags))
        {
            return null;
        }

        var values = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value.Replace(" ", string.Empty, StringComparison.Ordinal));
        }

        return result.Count == 0 ? null : result;
    }
}

public sealed class ChannelPriceInput
{
    [Display(Name = "قیمت سرو در محل")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? DineIn { get; set; }

    [Display(Name = "قیمت بیرون‌بر")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? TakeAway { get; set; }

    [Display(Name = "قیمت دلیوری")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? Delivery { get; set; }

    [Display(Name = "قیمت انحصاری آنلاین")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? OnlineExclusive { get; set; }

    public IDictionary<string, decimal>? ToDictionary()
    {
        var dictionary = new Dictionary<string, decimal>();

        if (DineIn.HasValue)
        {
            dictionary[nameof(MenuChannel.DineIn)] = DineIn.Value;
        }

        if (TakeAway.HasValue)
        {
            dictionary[nameof(MenuChannel.TakeAway)] = TakeAway.Value;
        }

        if (Delivery.HasValue)
        {
            dictionary[nameof(MenuChannel.Delivery)] = Delivery.Value;
        }

        if (OnlineExclusive.HasValue)
        {
            dictionary[nameof(MenuChannel.OnlineExclusive)] = OnlineExclusive.Value;
        }

        return dictionary.Count == 0 ? null : dictionary;
    }
}

public sealed class ReorderItemsInput
{
    [Required]
    public IReadOnlyList<Guid> ItemIds { get; init; } = Array.Empty<Guid>();
}
