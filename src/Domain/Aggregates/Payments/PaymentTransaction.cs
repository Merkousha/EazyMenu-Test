using System;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Payments;

public sealed class PaymentTransaction : Entity<PaymentId>, IAggregateRoot
{
    private PaymentTransaction(
        PaymentId id,
        TenantId tenantId,
        Guid? subscriptionId,
        Money amount,
        Money originalAmount,
        PaymentMethod method,
        string description,
        string? discountCode,
        Percentage? discountPercentage,
        DateTime issuedAtUtc,
        string? gatewayAuthority)
        : base(id)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new DomainException("شناسه مستاجر پرداخت معتبر نیست.");
        }

        if (method == PaymentMethod.Unspecified)
        {
            throw new DomainException("روش پرداخت نامعتبر است.");
        }

        if (amount.IsGreaterThan(originalAmount))
        {
            throw new DomainException("مبلغ نهایی نمی‌تواند از مبلغ اصلی بیشتر باشد.");
        }

        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        OriginalAmount = originalAmount;
        Method = method;
        Description = description.Trim();
        DiscountCode = string.IsNullOrWhiteSpace(discountCode) ? null : discountCode.Trim();
        DiscountPercentage = discountPercentage;
        IssuedAtUtc = issuedAtUtc;
        GatewayAuthority = string.IsNullOrWhiteSpace(gatewayAuthority) ? null : gatewayAuthority.Trim();
        Status = PaymentStatus.Pending;
    }

    private PaymentTransaction()
    {
    }

    public TenantId TenantId { get; private set; }

    public Guid? SubscriptionId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public Money OriginalAmount { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? DiscountCode { get; private set; }

    public Percentage? DiscountPercentage { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? RefundedAtUtc { get; private set; }

    public string? ExternalReference { get; private set; }

    public string? FailureReason { get; private set; }

    public string? GatewayAuthority { get; private set; }

    public Money DiscountAmount => OriginalAmount.Subtract(Amount);

    public bool HasDiscount => DiscountPercentage is not null || !string.IsNullOrWhiteSpace(DiscountCode);

    public static PaymentTransaction Issue(
        TenantId tenantId,
        Guid? subscriptionId,
        Money amount,
        PaymentMethod method,
        string description,
        DateTime issuedAtUtc,
        Money? originalAmount = null,
        Percentage? discountPercentage = null,
        string? discountCode = null,
        string? gatewayAuthority = null)
    {
        Guard.AgainstNull(amount, nameof(amount));
        Guard.AgainstNullOrWhiteSpace(description, nameof(description));

        var baselineAmount = originalAmount ?? amount;

        return new PaymentTransaction(
            PaymentId.New(),
            tenantId,
            subscriptionId,
            amount,
            baselineAmount,
            method,
            description,
            discountCode,
            discountPercentage,
            issuedAtUtc,
            gatewayAuthority);
    }

    public void AttachGatewayAuthority(string authority)
    {
        Guard.AgainstNullOrWhiteSpace(authority, nameof(authority));
        EnsureStatus(PaymentStatus.Pending);
        GatewayAuthority = authority.Trim();
    }

    public void MarkSucceeded(string externalReference, DateTime completedAtUtc)
    {
        Guard.AgainstNullOrWhiteSpace(externalReference, nameof(externalReference));
        EnsureStatus(PaymentStatus.Pending);
        EnsureCompletionTimestamp(completedAtUtc);

        Status = PaymentStatus.Succeeded;
        ExternalReference = externalReference.Trim();
        CompletedAtUtc = completedAtUtc;
        FailureReason = null;
    }

    public void MarkFailed(string failureReason, DateTime failedAtUtc)
    {
        Guard.AgainstNullOrWhiteSpace(failureReason, nameof(failureReason));
        EnsureStatus(PaymentStatus.Pending);
        EnsureCompletionTimestamp(failedAtUtc);

        Status = PaymentStatus.Failed;
        FailureReason = failureReason.Trim();
        CompletedAtUtc = failedAtUtc;
    }

    public void MarkRefunded(DateTime refundedAtUtc, string? externalReference = null)
    {
        EnsureStatus(PaymentStatus.Succeeded);
        EnsureCompletionTimestamp(refundedAtUtc);

        Status = PaymentStatus.Refunded;
        RefundedAtUtc = refundedAtUtc;
        CompletedAtUtc ??= refundedAtUtc;

        if (!string.IsNullOrWhiteSpace(externalReference))
        {
            ExternalReference = externalReference.Trim();
        }
    }

    private void EnsureStatus(params PaymentStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new DomainException("تغییر وضعیت تراکنش مجاز نیست.");
        }
    }

    private void EnsureCompletionTimestamp(DateTime timestamp)
    {
        if (timestamp < IssuedAtUtc)
        {
            throw new DomainException("زمان تکمیل نمی‌تواند قبل از صدور باشد.");
        }
    }
}
