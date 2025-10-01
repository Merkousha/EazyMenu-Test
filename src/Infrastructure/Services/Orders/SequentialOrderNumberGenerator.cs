using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Infrastructure.Services.Orders;

/// <summary>
/// Generates sequential order numbers in format: ORD-YYYYMMDD-NNNN
/// </summary>
internal sealed class SequentialOrderNumberGenerator : IOrderNumberGenerator
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static int _counter = 0;
    private static string _lastDate = string.Empty;

    public Task<string> GenerateAsync(TenantId tenantId, DateTime timestampUtc, CancellationToken cancellationToken = default)
    {
        _lock.Wait(cancellationToken);
        try
        {
            var currentDate = timestampUtc.ToString("yyyyMMdd");
            
            // Reset counter if date changed
            if (_lastDate != currentDate)
            {
                _counter = 0;
                _lastDate = currentDate;
            }

            _counter++;
            var orderNumber = $"ORD-{currentDate}-{_counter:D4}";
            
            return Task.FromResult(orderNumber);
        }
        finally
        {
            _lock.Release();
        }
    }
}
