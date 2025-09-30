using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Application.Common.Time;
using EazyMenu.Domain.Common.Exceptions;
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

    public RequestCustomerLoginCommandHandler(
        IOneTimePasswordGenerator otpGenerator,
        IOneTimePasswordStore otpStore,
        ISmsSender smsSender,
        ISmsFailureAlertService smsFailureAlertService,
        IDateTimeProvider dateTimeProvider)
    {
        _otpGenerator = otpGenerator;
        _otpStore = otpStore;
        _smsSender = smsSender;
        _smsFailureAlertService = smsFailureAlertService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RequestCustomerLoginResult> HandleAsync(RequestCustomerLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new BusinessRuleViolationException("شماره موبایل الزامی است.");
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

        var message = $"کد ورود شما به ایزی‌منو: {verificationCode}";
        try
        {
            await _smsSender.SendAsync(phoneNumber.Value, message, cancellationToken);
        }
        catch (Exception exception)
        {
            await _smsFailureAlertService.NotifyFailureAsync(phoneNumber.Value, message, exception, cancellationToken);
            throw;
        }

        return new RequestCustomerLoginResult(phoneNumber.Value, expiresAt, "sms");
    }
}
