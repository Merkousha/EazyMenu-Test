using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Web.Models.SiteWizard;

public sealed class SiteWizardViewModel
{
    [Required(ErrorMessage = "نام نمایشی الزامی است")]
    [StringLength(200, ErrorMessage = "نام نمایشی نباید بیشتر از 200 کاراکتر باشد")]
    public string DisplayName { get; set; } = string.Empty;

    [Url(ErrorMessage = "آدرس لوگو معتبر نیست")]
    [StringLength(512, ErrorMessage = "آدرس لوگو نباید بیشتر از 512 کاراکتر باشد")]
    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "رنگ اصلی الزامی است")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "رنگ باید به فرمت hex باشد (مثلاً #FF6B35)")]
    public string PrimaryColor { get; set; } = "#FF6B35";

    [Required(ErrorMessage = "رنگ ثانویه الزامی است")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "رنگ باید به فرمت hex باشد (مثلاً #004E89)")]
    public string SecondaryColor { get; set; } = "#004E89";

    [Url(ErrorMessage = "آدرس بنر معتبر نیست")]
    [StringLength(512, ErrorMessage = "آدرس بنر نباید بیشتر از 512 کاراکتر باشد")]
    public string? BannerImageUrl { get; set; }

    [StringLength(2000, ErrorMessage = "متن درباره ما نباید بیشتر از 2000 کاراکتر باشد")]
    public string? AboutText { get; set; }

    [StringLength(500, ErrorMessage = "ساعات کاری نباید بیشتر از 500 کاراکتر باشد")]
    public string? OpeningHours { get; set; }

    [Required(ErrorMessage = "قالب الزامی است")]
    [StringLength(100, ErrorMessage = "نام قالب نباید بیشتر از 100 کاراکتر باشد")]
    public string TemplateName { get; set; } = "classic";

    public bool IsPublished { get; set; }

    public bool? ShouldPublish { get; set; }
}
