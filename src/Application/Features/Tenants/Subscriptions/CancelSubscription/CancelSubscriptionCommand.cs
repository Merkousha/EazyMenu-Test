using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid TenantId) : ICommand<bool>;
