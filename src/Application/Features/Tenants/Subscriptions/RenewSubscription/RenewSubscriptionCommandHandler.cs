using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.RenewSubscription;

public sealed class RenewSubscriptionCommandHandler : ICommandHandler<RenewSubscriptionCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenewSubscriptionCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(RenewSubscriptionCommand command, CancellationToken cancellationToken = default)
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
            throw new BusinessRuleViolationException("هیچ اشتراک فعالی برای تمدید وجود ندارد.");
        }

        var activeSubscription = tenant.ActiveSubscription;

        if (command.NewStartDateUtc <= activeSubscription.StartDateUtc)
        {
            throw new BusinessRuleViolationException("تاریخ شروع تمدید باید بعد از شروع اشتراک فعلی باشد.");
        }

        if (activeSubscription.EndDateUtc is not null && command.NewStartDateUtc < activeSubscription.EndDateUtc.Value)
        {
            throw new BusinessRuleViolationException("تاریخ شروع تمدید نمی‌تواند قبل از پایان اشتراک فعلی باشد.");
        }

        var newPrice = Money.From(command.PriceAmount, command.PriceCurrency ?? Money.DefaultCurrency);

        var renewal = tenant.RenewSubscription(command.Plan, command.NewStartDateUtc, command.NewEndDateUtc, newPrice, command.AsTrial);

        if (command.DiscountPercentage is not null)
        {
            var percentage = Percentage.From(command.DiscountPercentage.Value);
            renewal.ApplyDiscount(percentage, command.DiscountCode);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return renewal.Id;
    }
}
