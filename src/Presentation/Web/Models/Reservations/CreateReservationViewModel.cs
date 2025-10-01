using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Web.Models.Reservations;

public sealed class CreateReservationViewModel
{
    public Guid TenantId { get; set; }
    
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "لطفاً روز هفته را انتخاب کنید.")]
    [Display(Name = "روز هفته")]
    public DayOfWeek DayOfWeek { get; set; } = DateTime.Today.DayOfWeek;

    [Required(ErrorMessage = "لطفاً ساعت شروع را وارد کنید.")]
    [Display(Name = "ساعت شروع")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "فرمت ساعت باید HH:mm باشد.")]
    public string StartTime { get; set; } = "12:00";

    [Required(ErrorMessage = "لطفاً ساعت پایان را وارد کنید.")]
    [Display(Name = "ساعت پایان")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "فرمت ساعت باید HH:mm باشد.")]
    public string EndTime { get; set; } = "14:00";

    [Required(ErrorMessage = "لطفاً تعداد نفرات را وارد کنید.")]
    [Range(1, 50, ErrorMessage = "تعداد نفرات باید بین 1 تا 50 باشد.")]
    [Display(Name = "تعداد نفرات")]
    public int PartySize { get; set; } = 2;

    [Display(Name = "ترجیح فضای باز")]
    public bool PrefersOutdoor { get; set; }

    [MaxLength(500, ErrorMessage = "درخواست خاص نمی‌تواند بیشتر از 500 کاراکتر باشد.")]
    [Display(Name = "درخواست خاص")]
    public string? SpecialRequest { get; set; }

    [MaxLength(200, ErrorMessage = "نام مشتری نمی‌تواند بیشتر از 200 کاراکتر باشد.")]
    [Display(Name = "نام مشتری")]
    public string? CustomerName { get; set; }

    [Phone(ErrorMessage = "شماره تلفن معتبر نیست.")]
    [Display(Name = "شماره تماس")]
    public string? CustomerPhone { get; set; }

    public IReadOnlyCollection<TableOptionViewModel> AvailableTables { get; set; } = Array.Empty<TableOptionViewModel>();
}

public sealed record TableOptionViewModel(
    Guid TableId,
    string Label,
    int Capacity,
    bool IsOutdoor);
