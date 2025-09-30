using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Common.Time;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Customers.Login;

public sealed class RequestCustomerLoginCommandHandler : ICommandHandler<RequestCustomerLoginCommand, RequestCustomerLoginResult>
{
    private const int VerificationCodeLength = 5;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(2);

    private readonly IOneTimePasswordGenerator _otpGenerator;
    private readonly IOneTimePasswordStore _otpStore;
    private readonly ISmsSender _smsSender;
    private readonly ISmsFailureAlertService _smsFailureAlertService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITenantRepository _tenantRepository;

    public RequestCustomerLoginCommandHandler(
        IOneTimePasswordGenerator otpGenerator,
        IOneTimePasswordStore otpStore,
        ISmsSender smsSender,
        ISmsFailureAlertService smsFailureAlertService,
        IDateTimeProvider dateTimeProvider,
        ITenantRepository tenantRepository)
    {
        _otpGenerator = otpGenerator;
        _otpStore = otpStore;
        _smsSender = smsSender;
        _smsFailureAlertService = smsFailureAlertService;
        _dateTimeProvider = dateTimeProvider;
        _tenantRepository = tenantRepository;
    }

    public async Task<RequestCustomerLoginResult> HandleAsync(RequestCustomerLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new BusinessRuleViolationException("شماره موبایل الزامی است.");
        }

        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر برای ارسال پیامک نامعتبر است.");
        }

        PhoneNumber phoneNumber;
        try
        {
            phoneNumber = PhoneNumber.Create(command.PhoneNumber);
        }
        catch (DomainException ex)
        {
            throw new BusinessRuleViolationException(ex.Message);
        }

        var verificationCode = _otpGenerator.GenerateNumericCode(VerificationCodeLength);
        var expiresAt = _dateTimeProvider.UtcNow.Add(CodeLifetime);

        await _otpStore.StoreAsync(phoneNumber.Value, verificationCode, expiresAt, cancellationToken);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new BusinessRuleViolationException("مستاجر یافت نشد.");
        }

        var subscriptionPlan = tenant.ActiveSubscription?.Plan ?? tenant.Subscriptions.OrderByDescending(s => s.StartDateUtc).FirstOrDefault()?.Plan ?? SubscriptionPlan.Trial;

        var context = new SmsSendContext(tenant.Id, subscriptionPlan);

        var message = $"کد ورود شما به ایزی‌منو: {verificationCode}";
        try
        {
            await _smsSender.SendAsync(phoneNumber.Value, message, context, cancellationToken);
        }
        catch (Exception exception)
        {
            await _smsFailureAlertService.NotifyFailureAsync(phoneNumber.Value, message, exception, context, cancellationToken);
            throw;
        }

        return new RequestCustomerLoginResult(phoneNumber.Value, expiresAt, "sms");
    }
}
