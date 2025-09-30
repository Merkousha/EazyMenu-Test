using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Customers.Login;

public sealed class VerifyCustomerLoginCommandHandler : ICommandHandler<VerifyCustomerLoginCommand, VerifyCustomerLoginResult>
{
    private readonly IOneTimePasswordStore _otpStore;

    public VerifyCustomerLoginCommandHandler(IOneTimePasswordStore otpStore)
    {
        _otpStore = otpStore;
    }

    public async Task<VerifyCustomerLoginResult> HandleAsync(VerifyCustomerLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new BusinessRuleViolationException("شماره موبایل الزامی است.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new BusinessRuleViolationException("کد تأیید را وارد کنید.");
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

        var trimmedCode = command.Code.Trim();
        var validationResult = await _otpStore.ValidateAsync(phoneNumber.Value, trimmedCode, cancellationToken);
        if (validationResult.IsValid)
        {
            return new VerifyCustomerLoginResult(true, phoneNumber.Value, null);
        }

        var failureReason = validationResult.IsExpired
            ? validationResult.FailureReason ?? "کد ارسال شده منقضی شده است."
            : validationResult.FailureReason ?? "کد وارد شده صحیح نیست.";

        return new VerifyCustomerLoginResult(false, phoneNumber.Value, failureReason);
    }
}
