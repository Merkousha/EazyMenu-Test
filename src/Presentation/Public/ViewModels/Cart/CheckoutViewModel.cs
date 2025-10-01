using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EazyMenu.Public.Models.Cart;

namespace EazyMenu.Public.ViewModels.Cart;

/// <summary>
/// ViewModel for the checkout page with customer information and fulfillment options.
/// </summary>
public sealed class CheckoutViewModel
{
    public List<CartItemModel> Items { get; set; } = new();
    public decimal SubtotalAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount => SubtotalAmount + DeliveryFee + TaxAmount;
    
    [Required(ErrorMessage = "لطفاً نام خود را وارد کنید.")]
    [StringLength(200, ErrorMessage = "نام نباید بیشتر از 200 کاراکتر باشد.")]
    [Display(Name = "نام و نام خانوادگی")]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "لطفاً شماره تلفن خود را وارد کنید.")]
    [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است.")]
    [Display(Name = "شماره تلفن")]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "لطفاً نوع تحویل را انتخاب کنید.")]
    [Display(Name = "نوع تحویل")]
    public string FulfillmentMethod { get; set; } = "DineIn";
    
    [StringLength(1000, ErrorMessage = "یادداشت نباید بیشتر از 1000 کاراکتر باشد.")]
    [Display(Name = "یادداشت (اختیاری)")]
    public string? CustomerNote { get; set; }
}
