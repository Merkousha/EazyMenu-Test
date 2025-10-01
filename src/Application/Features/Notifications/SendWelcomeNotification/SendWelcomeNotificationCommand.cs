using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Notifications.SendWelcomeNotification;

/// <summary>
/// Command to send welcome notification (SMS and Email) to a newly registered tenant.
/// </summary>
public sealed record SendWelcomeNotificationCommand(
    Guid TenantId,
    string RestaurantName,
    string ManagerEmail,
    string ManagerPhone,
    string PlanName,
    DateTime SubscriptionEndDate,
    bool IsTrial) : ICommand;
