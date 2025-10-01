using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.Common;

namespace EazyMenu.Application.Features.Orders.GetOrderDetails;

public sealed record GetOrderDetailsQuery(Guid TenantId, Guid OrderId) : IQuery<OrderDetailsDto>;
