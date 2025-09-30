namespace EazyMenu.Web.Models;

public sealed class PaymentCallbackViewModel
{
    public bool IsSuccessful { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? ReferenceCode { get; init; }

    public string? TenantId { get; init; }

    public string? SubscriptionId { get; init; }
}
