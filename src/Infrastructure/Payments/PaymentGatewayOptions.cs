using System;

namespace EazyMenu.Infrastructure.Payments;

public sealed class PaymentGatewayOptions
{
    public string CallbackUri { get; set; } = "https://eazymenu.ir/payments/callback";

    public Uri GetCallbackUri()
    {
        if (Uri.TryCreate(CallbackUri, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        return new Uri("https://eazymenu.ir/payments/callback", UriKind.Absolute);
    }
}
