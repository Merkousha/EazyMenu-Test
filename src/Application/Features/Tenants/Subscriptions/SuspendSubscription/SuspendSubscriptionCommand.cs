using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.SuspendSubscription;

public sealed record SuspendSubscriptionCommand(Guid TenantId) : ICommand<bool>;
