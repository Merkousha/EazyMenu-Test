using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public PaymentTransactionRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PaymentTransaction?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PaymentTransactions
            .AsTracking()
            .FirstOrDefaultAsync(transaction => transaction.Id == paymentId, cancellationToken);
    }

    public Task<PaymentTransaction?> GetByGatewayAuthorityAsync(string authority, CancellationToken cancellationToken = default)
    {
        return _dbContext.PaymentTransactions
            .AsTracking()
            .FirstOrDefaultAsync(transaction => transaction.GatewayAuthority == authority, cancellationToken);
    }

    public Task UpdateAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken = default)
    {
        _dbContext.PaymentTransactions.Update(paymentTransaction);
        return Task.CompletedTask;
    }
}
