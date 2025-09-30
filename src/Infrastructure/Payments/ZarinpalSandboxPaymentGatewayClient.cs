using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Payments;

namespace EazyMenu.Infrastructure.Payments;

internal sealed class ZarinpalSandboxPaymentGatewayClient : IPaymentGatewayClient
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ZarinpalSandboxPaymentGatewayClient(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<PaymentGatewayResponse> CreatePaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var authority = $"SANDBOX-{Guid.NewGuid():N}";
        var redirectUri = new Uri($"https://sandbox.zarinpal.com/pg/StartPay/{authority}", UriKind.Absolute);
        var expiresAt = _dateTimeProvider.UtcNow.AddMinutes(30);

        return Task.FromResult(new PaymentGatewayResponse(authority, redirectUri, expiresAt));
    }

    public Task<PaymentVerificationResponse> VerifyPaymentAsync(PaymentVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Authority.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentVerificationResponse(false, null, "پرداخت توسط کاربر لغو شد."));
        }

        if (request.Authority.Contains("EXPIRE", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentVerificationResponse(false, null, "مهلت پرداخت منقضی شده است."));
        }

        var reference = $"ZP-REF-{Guid.NewGuid():N}".ToUpperInvariant();
        return Task.FromResult(new PaymentVerificationResponse(true, reference, null));
    }
}
