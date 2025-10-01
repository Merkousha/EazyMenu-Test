using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Application.Features.Payments.VerifyPayment;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

[Route("payments")]
public sealed class PaymentsController : Controller
{
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult> _verifyPaymentCommandHandler;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentTransactionRepository paymentTransactionRepository,
        ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult> verifyPaymentCommandHandler,
        ILogger<PaymentsController> logger)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _verifyPaymentCommandHandler = verifyPaymentCommandHandler;
        _logger = logger;
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? authority, string? status, Guid? paymentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authority) && paymentId is null)
        {
            _logger.LogWarning("Payment callback invoked without authority or paymentId.");
            return View("Callback", new PaymentCallbackViewModel
            {
                IsSuccessful = false,
                Title = "پرداخت ناموفق",
                Message = "اطلاعات لازم برای اعتبارسنجی پرداخت دریافت نشد.",
            });
        }

        var paymentTransaction = await ResolvePaymentAsync(authority, paymentId, cancellationToken);
        if (paymentTransaction is null)
        {
            _logger.LogWarning("Payment callback could not find transaction. Authority: {Authority}, PaymentId: {PaymentId}", authority, paymentId);
            return View("Callback", new PaymentCallbackViewModel
            {
                IsSuccessful = false,
                Title = "پرداخت ناموفق",
                Message = "تراکنش مربوطه یافت نشد. لطفاً با پشتیبانی تماس بگیرید.",
            });
        }

        var gatewayAuthority = paymentTransaction.GatewayAuthority ?? authority ?? string.Empty;

        var command = new VerifyPaymentCommand(
            paymentTransaction.Id.Value,
            gatewayAuthority,
            status ?? string.Empty);

        try
        {
            var result = await _verifyPaymentCommandHandler.HandleAsync(command, cancellationToken);

            if (result.IsSuccessful)
            {
                return View("Callback", new PaymentCallbackViewModel
                {
                    IsSuccessful = true,
                    Title = "پرداخت موفق",
                    Message = "پرداخت شما با موفقیت تأیید شد و اشتراک فعال گردید.",
                    ReferenceCode = result.ReferenceCode,
                    TenantId = paymentTransaction.TenantId.Value.ToString(),
                    SubscriptionId = paymentTransaction.SubscriptionId?.ToString()
                });
            }

            return View("Callback", new PaymentCallbackViewModel
            {
                IsSuccessful = false,
                Title = "پرداخت ناموفق",
                Message = string.IsNullOrWhiteSpace(result.FailureReason)
                    ? "پرداخت توسط درگاه تأیید نشد."
                    : result.FailureReason,
                ReferenceCode = result.ReferenceCode,
                TenantId = paymentTransaction.TenantId.Value.ToString(),
                SubscriptionId = paymentTransaction.SubscriptionId?.ToString()
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Payment verification violated business rule. Authority: {Authority}, PaymentId: {PaymentId}", authority, paymentId);
            return View("Callback", new PaymentCallbackViewModel
            {
                IsSuccessful = false,
                Title = "پرداخت ناموفق",
                Message = ex.Message,
                TenantId = paymentTransaction.TenantId.Value.ToString(),
                SubscriptionId = paymentTransaction.SubscriptionId?.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying payment. Authority: {Authority}, PaymentId: {PaymentId}", authority, paymentId);
            return View("Callback", new PaymentCallbackViewModel
            {
                IsSuccessful = false,
                Title = "خطای غیرمنتظره",
                Message = "در پردازش پرداخت خطایی رخ داده است. لطفاً مجدداً تلاش کنید یا با پشتیبانی تماس بگیرید.",
                TenantId = paymentTransaction.TenantId.Value.ToString(),
                SubscriptionId = paymentTransaction.SubscriptionId?.ToString()
            });
        }
    }

    private async Task<Domain.Aggregates.Payments.PaymentTransaction?> ResolvePaymentAsync(string? authority, Guid? paymentId, CancellationToken cancellationToken)
    {
        if (paymentId is Guid rawPaymentId && PaymentId.TryCreate(rawPaymentId, out var typedPaymentId))
        {
            var byId = await _paymentTransactionRepository.GetByIdAsync(typedPaymentId, cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        if (!string.IsNullOrWhiteSpace(authority))
        {
            var byAuthority = await _paymentTransactionRepository.GetByGatewayAuthorityAsync(authority.Trim(), cancellationToken);
            if (byAuthority is not null)
            {
                return byAuthority;
            }
        }

        return null;
    }
}
