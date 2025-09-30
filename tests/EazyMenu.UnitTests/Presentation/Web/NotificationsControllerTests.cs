using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;
using EazyMenu.Web.Controllers;
using EazyMenu.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Web;

public sealed class NotificationsControllerTests
{
    [Fact]
    public async Task SmsLogs_ReturnsViewWithMappedItems()
    {
        var logs = new List<SmsDeliveryRecord>
        {
            new(
                Guid.NewGuid(),
                "09120000001",
                "کد ورود شما 1234",
                "logger",
                SmsDeliveryStatus.Sent,
                new DateTimeOffset(2025, 10, 2, 12, 0, 0, TimeSpan.Zero)),
            new(
                Guid.NewGuid(),
                "09120000002",
                "پرداخت شما ناموفق بود",
                "kavenegar",
                SmsDeliveryStatus.Failed,
                new DateTimeOffset(2025, 10, 2, 12, 5, 0, TimeSpan.Zero),
                ErrorCode: "500",
                ErrorMessage: "Gateway error")
        };

    var response = new SmsDeliveryLogPage(logs, HasMore: true);
        var handler = new StubGetSmsDeliveryLogsQueryHandler(response);
        var controller = new NotificationsController(NullLogger<NotificationsController>.Instance, handler);

        var result = await controller.SmsLogs(page: 2, status: SmsDeliveryStatus.Sent, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SmsDeliveryLogsViewModel>(viewResult.Model);

        Assert.Equal(2, model.Page);
        Assert.Equal(SmsDeliveryStatus.Sent, model.Status);
        Assert.True(model.HasMore);
        Assert.Equal(2, model.Items.Count);
        Assert.Equal("logger", model.Items[0].Provider);
        Assert.Equal("Gateway error", model.Items[1].ErrorMessage);

        Assert.NotNull(handler.LastQuery);
        Assert.Equal(2, handler.LastQuery!.Page);
        Assert.Equal(SmsDeliveryStatus.Sent, handler.LastQuery.Status);
    }

    [Fact]
    public async Task SmsLogs_WhenHandlerThrows_AddsModelError()
    {
        var handler = new ThrowingGetSmsDeliveryLogsQueryHandler();
        var controller = new NotificationsController(NullLogger<NotificationsController>.Instance, handler);

        var result = await controller.SmsLogs(page: 1, status: null, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SmsDeliveryLogsViewModel>(viewResult.Model);

        Assert.Empty(model.Items);
        Assert.False(model.HasMore);
        Assert.False(controller.ModelState.IsValid);
    }

    private sealed class StubGetSmsDeliveryLogsQueryHandler : IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage>
    {
        private readonly SmsDeliveryLogPage _response;

        public StubGetSmsDeliveryLogsQueryHandler(SmsDeliveryLogPage response)
        {
            _response = response;
        }

        public GetSmsDeliveryLogsQuery? LastQuery { get; private set; }

        public Task<SmsDeliveryLogPage> HandleAsync(GetSmsDeliveryLogsQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(_response);
        }
    }

    private sealed class ThrowingGetSmsDeliveryLogsQueryHandler : IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage>
    {
        public Task<SmsDeliveryLogPage> HandleAsync(GetSmsDeliveryLogsQuery query, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("failure");
        }
    }
}
