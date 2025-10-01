using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.Common;

namespace EazyMenu.Application.Features.Orders.GetOrders;

public sealed record GetOrdersQuery(
    Guid TenantId,
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<IReadOnlyCollection<OrderSummaryDto>>;
