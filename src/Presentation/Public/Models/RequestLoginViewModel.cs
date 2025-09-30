using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Public.Models;

public sealed class RequestLoginViewModel
{
    [Display(Name = "شماره موبایل")]
    [Required(ErrorMessage = "{0} را وارد کنید.")]
    [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "شماره موبایل معتبر نیست.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
