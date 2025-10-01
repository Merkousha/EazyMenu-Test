using System.ComponentModel.DataAnnotations;

namespace EazyMenu.Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "نام کامل الزامی است.")]
    [StringLength(200, ErrorMessage = "نام کامل نباید بیشتر از 200 کاراکتر باشد.")]
    [Display(Name = "نام کامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست.")]
    [StringLength(256)]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست.")]
    [StringLength(15)]
    [Display(Name = "شماره تلفن")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تایید رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تایید آن یکسان نیستند.")]
    [Display(Name = "تایید رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
