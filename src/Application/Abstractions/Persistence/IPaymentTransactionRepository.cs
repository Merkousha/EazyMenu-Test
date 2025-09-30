using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default);

    Task UpdateAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken = default);
}
