using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Public.Models;

public sealed class VerifyLoginViewModel
{
    [Required(ErrorMessage = "شماره موبایل موجود نیست.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "کد تأیید")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [RegularExpression(@"^\d{4,6}$", ErrorMessage = "کد تأیید باید عددی باشد.")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
