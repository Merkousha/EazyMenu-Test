using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Application.Common.Time;
using EazyMenu.Application.Features.Customers.Login;
using EazyMenu.Domain.Common.Exceptions;

namespace EazyMenu.UnitTests.Application.Features.Customers;

public sealed class CustomerSmsLoginTests
{
    private static readonly DateTime FixedNow = new(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RequestLogin_ValidPhone_StoresAndSendsCode()
    {
        var generator = new FakeOtpGenerator("12345");
        var store = new FakeOtpStore();
        var sms = new FakeSmsSender();
        var handler = new RequestCustomerLoginCommandHandler(generator, store, sms, new FixedDateTimeProvider(FixedNow));

        var result = await handler.HandleAsync(new RequestCustomerLoginCommand("+989121234567"));

        Assert.Equal("+989121234567", result.PhoneNumber);
        Assert.Equal(FixedNow.AddMinutes(2), result.ExpiresAtUtc);
        Assert.Equal("sms", result.DeliveryChannel);
        Assert.Equal("12345", store.StoredCode);
        Assert.Equal(FixedNow.AddMinutes(2), store.StoredExpiry);
        Assert.Equal("+989121234567", sms.LastPhoneNumber);
        Assert.Contains("12345", sms.LastMessage);
    }

    [Fact]
    public async Task RequestLogin_InvalidPhone_Throws()
    {
        var handler = new RequestCustomerLoginCommandHandler(
            new FakeOtpGenerator("12345"),
            new FakeOtpStore(),
            new FakeSmsSender(),
            new FixedDateTimeProvider(FixedNow));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(new RequestCustomerLoginCommand("invalid")));
    }

    [Fact]
    public async Task VerifyLogin_WithValidCode_ReturnsAuthenticated()
    {
        var store = new FakeOtpStore
        {
            ValidationResult = OneTimePasswordValidationResult.Success()
        };
        var handler = new VerifyCustomerLoginCommandHandler(store);

        var result = await handler.HandleAsync(new VerifyCustomerLoginCommand("+989121234567", "12345"));

        Assert.True(result.IsAuthenticated);
        Assert.Equal("+989121234567", result.PhoneNumber);
        Assert.Null(result.FailureReason);
        Assert.Equal("+989121234567", store.ValidatedPhone);
        Assert.Equal("12345", store.ValidatedCode);
    }

    [Fact]
    public async Task VerifyLogin_InvalidCode_ReturnsFailure()
    {
        var store = new FakeOtpStore
        {
            ValidationResult = OneTimePasswordValidationResult.Invalid("کد نادرست است.")
        };
        var handler = new VerifyCustomerLoginCommandHandler(store);

        var result = await handler.HandleAsync(new VerifyCustomerLoginCommand("+989121234567", "00000"));

        Assert.False(result.IsAuthenticated);
        Assert.Equal("+989121234567", result.PhoneNumber);
        Assert.Equal("کد نادرست است.", result.FailureReason);
    }

    [Fact]
    public async Task VerifyLogin_InvalidPhone_Throws()
    {
        var handler = new VerifyCustomerLoginCommandHandler(new FakeOtpStore());

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(new VerifyCustomerLoginCommand("bad", "12345")));
    }

    private sealed class FakeOtpGenerator : IOneTimePasswordGenerator
    {
        private readonly string _code;

        public FakeOtpGenerator(string code)
        {
            _code = code;
        }

        public string GenerateNumericCode(int length) => _code;
    }

    private sealed class FakeOtpStore : IOneTimePasswordStore
    {
        public string? StoredPhone { get; private set; }
        public string? StoredCode { get; private set; }
        public DateTime? StoredExpiry { get; private set; }
        public string? ValidatedPhone { get; private set; }
        public string? ValidatedCode { get; private set; }
        public OneTimePasswordValidationResult ValidationResult { get; set; } = OneTimePasswordValidationResult.Invalid();

        public Task StoreAsync(string phoneNumber, string code, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            StoredPhone = phoneNumber;
            StoredCode = code;
            StoredExpiry = expiresAtUtc;
            return Task.CompletedTask;
        }

        public Task<OneTimePasswordValidationResult> ValidateAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
        {
            ValidatedPhone = phoneNumber;
            ValidatedCode = code;
            return Task.FromResult(ValidationResult);
        }
    }

    private sealed class FakeSmsSender : ISmsSender
    {
        public string? LastPhoneNumber { get; private set; }
        public string? LastMessage { get; private set; }

        public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            LastPhoneNumber = phoneNumber;
            LastMessage = message;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        private readonly DateTime _utcNow;

        public FixedDateTimeProvider(DateTime utcNow)
        {
            _utcNow = utcNow;
        }

        public DateTime UtcNow => _utcNow;
    }
}
