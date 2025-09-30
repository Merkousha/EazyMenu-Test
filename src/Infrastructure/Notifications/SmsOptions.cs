using System;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class SmsOptions
{
    public string Provider { get; set; } = nameof(SmsProvider.Logging);

    public string? KavenegarApiKey { get; set; }

    public string? KavenegarSenderLine { get; set; }

    public SmsProvider GetProvider()
    {
        if (Enum.TryParse<SmsProvider>(Provider, true, out var parsed))
        {
            return parsed;
        }

        return SmsProvider.Logging;
    }
}
