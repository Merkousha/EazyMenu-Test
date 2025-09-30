using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Payments;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Payments.VerifyPayment;

public sealed class VerifyPaymentCommandHandler : ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult>
{
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VerifyPaymentCommandHandler(
        IPaymentTransactionRepository paymentTransactionRepository,
        ITenantRepository tenantRepository,
        IPaymentGatewayClient paymentGatewayClient,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _tenantRepository = tenantRepository;
        _paymentGatewayClient = paymentGatewayClient;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<VerifyPaymentResult> HandleAsync(VerifyPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (!PaymentId.TryCreate(command.PaymentId, out var paymentId))
        {
            throw new BusinessRuleViolationException("شناسه پرداخت معتبر نیست.");
        }

        if (string.IsNullOrWhiteSpace(command.Authority))
        {
            throw new BusinessRuleViolationException("شناسه مرجع زرین‌پال ارسال نشده است.");
        }

        var paymentTransaction = await _paymentTransactionRepository.GetByIdAsync(paymentId, cancellationToken);
        if (paymentTransaction is null)
        {
            throw new NotFoundException("تراکنش پرداخت یافت نشد.");
        }

        if (paymentTransaction.Status == PaymentStatus.Succeeded)
        {
            return new VerifyPaymentResult(true, paymentTransaction.Status, paymentTransaction.ExternalReference, null);
        }

        if (paymentTransaction.Status != PaymentStatus.Pending)
        {
            return new VerifyPaymentResult(false, paymentTransaction.Status, paymentTransaction.ExternalReference, paymentTransaction.FailureReason);
        }

        if (string.IsNullOrWhiteSpace(paymentTransaction.GatewayAuthority))
        {
            throw new BusinessRuleViolationException("تراکنش درگاه معتبری ندارد.");
        }

        if (!paymentTransaction.GatewayAuthority.Equals(command.Authority, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleViolationException("شناسه درگاه با تراکنش مطابقت ندارد.");
        }

        var verificationRequest = new PaymentVerificationRequest(paymentTransaction.GatewayAuthority, paymentTransaction.Amount);
        var verificationResponse = await _paymentGatewayClient.VerifyPaymentAsync(verificationRequest, cancellationToken);

        if (verificationResponse.IsSuccessful)
        {
            var referenceCode = string.IsNullOrWhiteSpace(verificationResponse.ReferenceId)
                ? $"ZP-{Guid.NewGuid():N}".ToUpperInvariant()
                : verificationResponse.ReferenceId.Trim();

            paymentTransaction.MarkSucceeded(referenceCode, _dateTimeProvider.UtcNow);

            if (paymentTransaction.SubscriptionId is Guid subscriptionId)
            {
                await ActivateSubscriptionAsync(paymentTransaction.TenantId, subscriptionId, cancellationToken);
            }

            await _paymentTransactionRepository.UpdateAsync(paymentTransaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new VerifyPaymentResult(true, paymentTransaction.Status, paymentTransaction.ExternalReference, null);
        }

        var failureReason = string.IsNullOrWhiteSpace(verificationResponse.FailureReason)
            ? "پرداخت توسط زرین‌پال تأیید نشد."
            : verificationResponse.FailureReason.Trim();

        paymentTransaction.MarkFailed(failureReason, _dateTimeProvider.UtcNow);

        await _paymentTransactionRepository.UpdateAsync(paymentTransaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyPaymentResult(false, paymentTransaction.Status, null, failureReason);
    }

    private async Task ActivateSubscriptionAsync(TenantId tenantId, Guid subscriptionId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new NotFoundException("مستاجر مربوط به این پرداخت یافت نشد.");
        }

        var subscription = tenant.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription is null)
        {
            throw new BusinessRuleViolationException("اشتراک مربوط به پرداخت در مستاجر یافت نشد.");
        }

        tenant.ActivateSubscription(subscription);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
    }
}
