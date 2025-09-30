using System;
using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Payments;

public sealed record PaymentGatewayRequest(
	TenantId TenantId,
	Guid? SubscriptionId,
	Money Amount,
	string Description,
	Uri CallbackUri,
	string? CustomerEmail,
	string? CustomerPhone,
	string? DiscountCode,
	IReadOnlyDictionary<string, string>? Metadata);
