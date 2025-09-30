using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EazyMenu.IntegrationTests.Infrastructure.Notifications;

public sealed class SmsFailureAlertServiceTests
{
    [Fact]
    public async Task NotifyFailureAsync_SendsEmailAndPersistsFallbackLog()
    {
        var options = new DbContextOptionsBuilder<EazyMenuDbContext>()
            .UseInMemoryDatabase($"SmsFailureFallback-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new EazyMenuDbContext(options);
        var store = new EfSmsDeliveryStore(dbContext);
        var emailSender = new FakeEmailSender();
        var alertNotifier = new FakeAlertNotifier();
        var service = new SmsFailureAlertService(
            emailSender,
            store,
            new FixedDateTimeProvider(new DateTime(2025, 10, 2, 10, 0, 0, DateTimeKind.Utc)),
            new EmailOptions { SupportAddress = "ops@eazymenu.ir" },
            alertNotifier,
            NullLogger<SmsFailureAlertService>.Instance);

        var exception = new InvalidOperationException("network down");
        var context = new SmsSendContext(Guid.Parse("22222222-2222-2222-2222-222222222222"), SubscriptionPlan.Pro);

        await service.NotifyFailureAsync("+989121234567", "پیام آزمایشی", exception, context, CancellationToken.None);

        Assert.Equal("ops@eazymenu.ir", emailSender.LastRecipient);
        Assert.Contains("ارسال پیامک تراکنشی", emailSender.LastBody);

        var logs = await dbContext.SmsDeliveryLogs.ToListAsync();
        var fallbackLog = Assert.Single(logs);
        Assert.Equal("fallback-email", fallbackLog.Provider);
        Assert.Equal(SmsDeliveryStatus.Sent, fallbackLog.Status);
        Assert.Equal("+989121234567", fallbackLog.PhoneNumber);
    Assert.Equal(context.TenantId, fallbackLog.TenantId);
    Assert.Equal(context.SubscriptionPlan, fallbackLog.SubscriptionPlan);

        Assert.NotNull(alertNotifier.LastAlert);
        Assert.Equal("+989121234567", alertNotifier.LastAlert!.PhoneNumber);
        Assert.Equal("email", alertNotifier.LastAlert.Channel);
    Assert.Equal(context.SubscriptionPlan, alertNotifier.LastAlert.SubscriptionPlan);
    Assert.Equal(context.TenantId, alertNotifier.LastAlert.TenantId);
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public string? LastRecipient { get; private set; }
        public string? LastSubject { get; private set; }
        public string? LastBody { get; private set; }

        public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
        {
            LastRecipient = recipient;
            LastSubject = subject;
            LastBody = body;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public FixedDateTimeProvider(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }

    private sealed class FakeAlertNotifier : ISmsFailureAlertNotifier
    {
        public SmsFailureAlert? LastAlert { get; private set; }

        public Task PublishAsync(SmsFailureAlert alert, CancellationToken cancellationToken = default)
        {
            LastAlert = alert;
            return Task.CompletedTask;
        }
    }
}
