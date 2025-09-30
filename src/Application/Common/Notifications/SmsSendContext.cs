using System;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// زمینه ارسال پیامک شامل اطلاعات مستاجر و پلن اشتراک جهت ثبت مصرف.
/// </summary>
public sealed record SmsSendContext(Guid TenantId, SubscriptionPlan SubscriptionPlan);
