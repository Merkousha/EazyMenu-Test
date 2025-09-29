using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Tenants;

public sealed class Subscription : Entity<Guid>
{
    private Subscription(Guid id, SubscriptionPlan plan, Money price, DateTime startDateUtc, DateTime? endDateUtc, bool isTrial)
        : base(id)
    {
        Plan = plan;
        Price = price;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        IsTrial = isTrial;
        Status = SubscriptionStatus.Pending;
    }

    public SubscriptionPlan Plan { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public Money Price { get; private set; }

    public DateTime StartDateUtc { get; private set; }

    public DateTime? EndDateUtc { get; private set; }

    public bool IsTrial { get; private set; }

    public Percentage? DiscountPercentage { get; private set; }

    public string? DiscountCode { get; private set; }

    public Money NetPrice => DiscountPercentage is null
        ? Price
        : Price.Subtract(Price.Multiply(DiscountPercentage.ToFraction()));

    public static Subscription Create(SubscriptionPlan plan, Money price, DateTime startDateUtc, DateTime? endDateUtc = null, bool isTrial = false)
    {
        Guard.AgainstNull(price, nameof(price));

        if (endDateUtc is not null && endDateUtc <= startDateUtc)
        {
            throw new DomainException("تاریخ پایان اشتراک باید بعد از تاریخ شروع باشد.");
        }

        return new Subscription(Guid.NewGuid(), plan, price, startDateUtc, endDateUtc, isTrial);
    }

    public void Activate()
    {
        EnsureCanTransitionTo(SubscriptionStatus.Active);

        Status = SubscriptionStatus.Active;
    }

    public void Suspend()
    {
        EnsureCanTransitionTo(SubscriptionStatus.Suspended);

        Status = SubscriptionStatus.Suspended;
    }

    public void Cancel()
    {
        EnsureCanTransitionTo(SubscriptionStatus.Cancelled);

        Status = SubscriptionStatus.Cancelled;
        EndDateUtc ??= DateTime.UtcNow;
    }

    public void MarkExpired(DateTime expiredAtUtc)
    {
        if (expiredAtUtc < StartDateUtc)
        {
            throw new DomainException("زمان انقضا نمی‌تواند قبل از شروع باشد.");
        }

        Status = SubscriptionStatus.Expired;
        EndDateUtc = expiredAtUtc;
    }

    public void Renew(SubscriptionPlan plan, DateTime newStartUtc, DateTime? newEndUtc, Money newPrice, bool asTrial = false)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));
        Guard.AgainstDefault(plan, nameof(plan));

        EnsureCanTransitionTo(SubscriptionStatus.Pending);

        if (newStartUtc <= StartDateUtc)
        {
            throw new DomainException("تاریخ شروع تمدید باید بعد از اشتراک جاری باشد.");
        }

        if (newEndUtc is not null && newEndUtc <= newStartUtc)
        {
            throw new DomainException("تاریخ پایان تمدید باید بعد از شروع باشد.");
        }

    Plan = plan;
        Price = newPrice;
        StartDateUtc = newStartUtc;
        EndDateUtc = newEndUtc;
        IsTrial = asTrial;
        Status = SubscriptionStatus.Pending;
        DiscountPercentage = null;
        DiscountCode = null;
    }

    public void ChangePlan(SubscriptionPlan plan)
    {
        Plan = plan;
    }

    public void ApplyDiscount(Percentage discount, string? discountCode = null)
    {
        Guard.AgainstNull(discount, nameof(discount));

        if (discount.Value >= 100)
        {
            throw new DomainException("درصد تخفیف نمی‌تواند ۱۰۰ یا بیشتر باشد.");
        }

        DiscountPercentage = discount;
        DiscountCode = string.IsNullOrWhiteSpace(discountCode) ? null : discountCode.Trim();
    }

    private void EnsureCanTransitionTo(SubscriptionStatus targetStatus)
    {
        if (Status == targetStatus)
        {
            return;
        }

        if (Status == SubscriptionStatus.Cancelled || Status == SubscriptionStatus.Expired)
        {
            throw new DomainException("اشتراک منقضی یا لغوشده قابل تغییر نیست.");
        }
    }
}
