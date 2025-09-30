using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.GetActiveSubscription;

public sealed class GetActiveSubscriptionQueryHandler : IQueryHandler<GetActiveSubscriptionQuery, SubscriptionDetailsDto?>
{
    private readonly ITenantRepository _tenantRepository;

    public GetActiveSubscriptionQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<SubscriptionDetailsDto?> HandleAsync(GetActiveSubscriptionQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new NotFoundException("مستاجر مورد نظر یافت نشد.");
        }

        if (tenant.ActiveSubscription is null)
        {
            return null;
        }

        var active = tenant.ActiveSubscription;

        return new SubscriptionDetailsDto(
            active.Id,
            active.Plan,
            active.Status,
            active.Price.Amount,
            active.Price.Currency,
            active.NetPrice.Amount,
            active.StartDateUtc,
            active.EndDateUtc,
            active.IsTrial,
            active.DiscountPercentage?.Value,
            active.DiscountCode);
    }
}
