using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.ExpireSubscription;

public sealed record ExpireSubscriptionCommand(Guid TenantId, DateTime ExpiredAtUtc) : ICommand<bool>;
