using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.ReinstateSubscription;

public sealed record ReinstateSubscriptionCommand(Guid TenantId) : ICommand<bool>;
