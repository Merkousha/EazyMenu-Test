using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Tenants.Common;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.GetActiveSubscription;

public sealed record GetActiveSubscriptionQuery(Guid TenantId) : IQuery<SubscriptionDetailsDto?>;
