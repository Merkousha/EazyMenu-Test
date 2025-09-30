using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class SignalRSmsFailureAlertNotifier : ISmsFailureAlertNotifier
{
    private readonly IHubContext<SmsAlertsHub> _hubContext;
    private readonly ILogger<SignalRSmsFailureAlertNotifier> _logger;

    public SignalRSmsFailureAlertNotifier(IHubContext<SmsAlertsHub> hubContext, ILogger<SignalRSmsFailureAlertNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(SmsFailureAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group("dashboard").SendAsync(
                "smsFailure",
                new
                {
                    phoneNumber = alert.PhoneNumber,
                    message = alert.Message,
                    occurredAt = alert.OccurredAt,
                    errorMessage = alert.ErrorMessage,
                    channel = alert.Channel,
                    tenantId = alert.TenantId,
                    subscriptionPlan = alert.SubscriptionPlan?.ToString()
                },
                cancellationToken);
        }
        catch (System.Exception exception)
        {
            _logger.LogError(exception, "ارسال اعلان SignalR برای شکست پیامک با مشکل مواجه شد.");
        }
    }
}
