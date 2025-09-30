using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;

public sealed class GetSmsDeliveryLogsQueryHandler : IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage>
{
    private readonly ISmsDeliveryLogReader _logReader;

    public GetSmsDeliveryLogsQueryHandler(ISmsDeliveryLogReader logReader)
    {
        _logReader = logReader;
    }

    public Task<SmsDeliveryLogPage> HandleAsync(GetSmsDeliveryLogsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Page), "شماره صفحه باید بزرگ‌تر از صفر باشد.");
        }

        if (query.PageSize is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(query.PageSize), "اندازه صفحه باید بین ۱ و ۲۰۰ باشد.");
        }

        var skip = (query.Page - 1) * query.PageSize;
        return _logReader.GetAsync(query.Status, skip, query.PageSize, cancellationToken);
    }
}
