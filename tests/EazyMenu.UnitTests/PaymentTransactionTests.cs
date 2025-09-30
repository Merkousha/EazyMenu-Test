using System;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests;

public class PaymentTransactionTests
{
    [Fact]
    public void Issue_WithDiscount_SetsPendingState()
    {
        var tenantId = TenantId.New();
        var originalAmount = Money.From(1_000_000m);
        var discount = Percentage.From(10);
        var netAmount = originalAmount.Subtract(originalAmount.Multiply(discount.ToFraction()));

        var issuedAt = DateTime.UtcNow;
        var transaction = PaymentTransaction.Issue(
            tenantId,
            Guid.NewGuid(),
            netAmount,
            PaymentMethod.Zarinpal,
            "حق اشتراک پلن حرفه‌ای",
            issuedAt,
            originalAmount,
            discount,
            "WELCOME10",
            null);

        Assert.Equal(PaymentStatus.Pending, transaction.Status);
        Assert.Equal(netAmount.Amount, transaction.Amount.Amount);
        Assert.Equal(originalAmount.Amount, transaction.OriginalAmount.Amount);
        Assert.True(transaction.HasDiscount);
        Assert.Equal("WELCOME10", transaction.DiscountCode);
        Assert.Equal(discount.Value, transaction.DiscountPercentage!.Value);
    }

    [Fact]
    public void MarkSucceeded_FromPending_SetsStatusAndReference()
    {
        var tenantId = TenantId.New();
        var amount = Money.From(500_000m);
        var issuedAt = DateTime.UtcNow;
        var transaction = PaymentTransaction.Issue(
            tenantId,
            Guid.NewGuid(),
            amount,
            PaymentMethod.Zarinpal,
            "پرداخت آزمایشی",
            issuedAt,
            amount,
            null,
            null,
            null);

        transaction.AttachGatewayAuthority("AUTH-123");

        var completedAt = issuedAt.AddMinutes(5);
        transaction.MarkSucceeded("REF-999", completedAt);

        Assert.Equal(PaymentStatus.Succeeded, transaction.Status);
        Assert.Equal("REF-999", transaction.ExternalReference);
        Assert.Equal(completedAt, transaction.CompletedAtUtc);
        Assert.Equal("AUTH-123", transaction.GatewayAuthority);
        Assert.Null(transaction.FailureReason);
    }

    [Fact]
    public void MarkFailed_FromPending_SetsFailureInfo()
    {
        var tenantId = TenantId.New();
        var amount = Money.From(750_000m);
        var issuedAt = DateTime.UtcNow;
        var transaction = PaymentTransaction.Issue(
            tenantId,
            Guid.NewGuid(),
            amount,
            PaymentMethod.Zarinpal,
            "پرداخت با خطا",
            issuedAt,
            amount,
            null,
            null,
            null);

        var failedAt = issuedAt.AddMinutes(2);
        transaction.MarkFailed("خطای درگاه", failedAt);

        Assert.Equal(PaymentStatus.Failed, transaction.Status);
        Assert.Equal("خطای درگاه", transaction.FailureReason);
        Assert.Equal(failedAt, transaction.CompletedAtUtc);
    }
}
