using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly EmailOptions _options;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger, EmailOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            recipient = _options.SupportAddress;
        }

        _logger.LogWarning(
            "ارسال ایمیل جایگزین به {Recipient}. موضوع: {Subject}. بدنه: {Body}",
            recipient,
            subject,
            body);

        return Task.CompletedTask;
    }
}
