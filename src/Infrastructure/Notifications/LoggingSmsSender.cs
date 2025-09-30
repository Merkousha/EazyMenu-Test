using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ارسال پیامک به {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
