using System;

namespace EazyMenu.Application.Common.Interfaces.Payments;

public sealed record PaymentGatewayResponse(
    string Authority,
    Uri RedirectUri,
    DateTime ExpiresAtUtc);
