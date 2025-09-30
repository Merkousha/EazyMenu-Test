using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Payments;

public interface IPaymentGatewayClient
{
    Task<PaymentGatewayResponse> CreatePaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
}
