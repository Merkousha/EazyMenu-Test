using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EazyMenu.Web.Models;

public sealed class RegisterTenantViewModel
{
    [Display(Name = "نام رستوران")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [StringLength(200, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string RestaurantName { get; set; } = string.Empty;

    [Display(Name = "ایمیل مدیر")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [EmailAddress(ErrorMessage = "نشانی ایمیل معتبر نیست.")]
    [StringLength(200, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string ManagerEmail { get; set; } = string.Empty;

    [Display(Name = "شماره تماس مدیر")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "شماره تماس معتبر نیست.")]
    public string ManagerPhone { get; set; } = string.Empty;

    [Display(Name = "طرح اشتراک")]
    [Required(ErrorMessage = "انتخاب {0} ضروری است.")]
    public string PlanCode { get; set; } = "starter";

    [Display(Name = "شهر")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [StringLength(100, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "آدرس کامل")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [StringLength(300, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string Street { get; set; } = string.Empty;

    [Display(Name = "کدپستی")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [StringLength(20, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string PostalCode { get; set; } = string.Empty;

    [Display(Name = "فعال‌سازی دوره آزمایشی ۱۴ روزه")]
    public bool UseTrial { get; set; }

    [Display(Name = "کد تخفیف (اختیاری)")]
    [StringLength(50, ErrorMessage = "{0} باید کمتر از {1} کاراکتر باشد.")]
    public string? DiscountCode { get; set; }

    public IReadOnlyCollection<SelectListItem> Plans { get; set; } = new List<SelectListItem>();
}
