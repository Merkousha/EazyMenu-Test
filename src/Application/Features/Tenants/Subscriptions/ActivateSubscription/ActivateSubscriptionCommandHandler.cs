using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler : ICommandHandler<ActivateSubscriptionCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSubscriptionCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(ActivateSubscriptionCommand command, CancellationToken cancellationToken = default)
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

        var price = Money.From(command.PriceAmount, command.PriceCurrency ?? Money.DefaultCurrency);
        var subscription = Subscription.Create(command.Plan, price, command.StartDateUtc, command.EndDateUtc, command.IsTrial);

        if (command.DiscountPercentage is not null)
        {
            var percentage = Percentage.From(command.DiscountPercentage.Value);
            subscription.ApplyDiscount(percentage, command.DiscountCode);
        }

        tenant.ActivateSubscription(subscription);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
