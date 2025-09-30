using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.ReinstateSubscription;

public sealed class ReinstateSubscriptionCommandHandler : ICommandHandler<ReinstateSubscriptionCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReinstateSubscriptionCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(ReinstateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
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
            throw new BusinessRuleViolationException("هیچ اشتراک فعالی برای بازگشت وجود ندارد.");
        }

        if (tenant.ActiveSubscription.Status != SubscriptionStatus.Suspended)
        {
            throw new BusinessRuleViolationException("اشتراک فعلی در حالت تعلیق نیست.");
        }

        tenant.ReinstateSuspendedSubscription();

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
