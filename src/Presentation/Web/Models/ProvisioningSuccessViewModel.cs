namespace EazyMenu.Web.Models;

public sealed class ProvisioningSuccessViewModel
{
    public string Title { get; set; } = "اشتراک فعال شد";
    public string Message { get; set; } = "اشتراک شما با موفقیت فعال گردید.";
    public string TenantId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
}
