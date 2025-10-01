using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Orders;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(TenantId tenantId, DateTime timestampUtc, CancellationToken cancellationToken = default);
}
