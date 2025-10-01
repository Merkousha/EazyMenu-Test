using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Web.Models.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز عبور فعلی الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور فعلی")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور جدید")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تایید رمز عبور جدید الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور جدید و تایید آن یکسان نیستند.")]
    [Display(Name = "تایید رمز عبور جدید")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
