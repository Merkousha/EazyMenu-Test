using System;
using System.Threading.Tasks;
using EazyMenu.Application;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Payments;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Application.Features.Payments.VerifyPayment;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EazyMenu.IntegrationTests;

public class RegisterTenantFlowTests
{
    [Fact]
    public async Task ProvisionTenant_WithTrial_ReturnsActiveSubscriptionWithoutPayment()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var handler = new RegisterTenantCommandHandler(provisioningService);

        var command = new RegisterTenantCommand(
            "Cafe Azadi",
            "owner@eazymenu.ir",
            "+989121234567",
            "starter",
            "Tehran",
            "Valiasr Ave",
            "1966731111",
            UseTrial: true,
            DiscountCode: "WELCOME10");

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result.TenantId.Value);
        Assert.NotEqual(Guid.Empty, result.SubscriptionId);
        Assert.Null(result.Payment);

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == result.TenantId);

        Assert.NotNull(tenant);
        Assert.NotNull(tenant!.ActiveSubscription);
        Assert.Equal(SubscriptionPlan.Starter, tenant.ActiveSubscription!.Plan);
        Assert.True(tenant.ActiveSubscription.IsTrial);
        Assert.Equal(SubscriptionStatus.Active, tenant.ActiveSubscription.Status);
        Assert.Single(tenant.Subscriptions);

        var provisioningRecord = await dbContext.TenantProvisionings.SingleAsync(r => r.TenantId == result.TenantId.Value);
        Assert.Equal(result.SubscriptionId, provisioningRecord.SubscriptionId);
        Assert.Null(provisioningRecord.PaymentId);
        Assert.True(provisioningRecord.UseTrial);

        var paymentCount = await dbContext.PaymentTransactions.CountAsync();
        Assert.Equal(0, paymentCount);
    }

    [Fact]
    public async Task ProvisionTenant_PaidPlan_CreatesPendingPayment()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var handler = new RegisterTenantCommandHandler(provisioningService);

        var command = new RegisterTenantCommand(
            "Bistro Tehran",
            "billing@eazymenu.ir",
            "+989301112233",
            "starter",
            "Tehran",
            "Enghelab St",
            "1966732222",
            UseTrial: false,
            DiscountCode: "WELCOME10");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result.Payment);
        Assert.Equal(PaymentStatus.Pending, result.Payment!.Status);
        Assert.NotNull(result.Payment.RedirectUri);
        Assert.False(string.IsNullOrWhiteSpace(result.Payment.GatewayAuthority));

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == result.TenantId);

        Assert.NotNull(tenant);
        Assert.Null(tenant!.ActiveSubscription);
        var pendingSubscription = Assert.Single(tenant.Subscriptions);
        Assert.Equal(result.SubscriptionId, pendingSubscription.Id);
        Assert.Equal(SubscriptionStatus.Pending, pendingSubscription.Status);
        Assert.False(pendingSubscription.IsTrial);

        var payment = await dbContext.PaymentTransactions.SingleAsync(p => p.Id == result.Payment.PaymentId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentMethod.Zarinpal, payment.Method);
        Assert.Equal(891_000m, payment.Amount.Amount);
        Assert.Equal(990_000m, payment.OriginalAmount.Amount);
        Assert.Equal("IRR", payment.Amount.Currency);
        Assert.NotNull(payment.GatewayAuthority);

        var provisioningRecord = await dbContext.TenantProvisionings.SingleAsync(r => r.TenantId == result.TenantId.Value);
        Assert.Equal(result.SubscriptionId, provisioningRecord.SubscriptionId);
        Assert.Equal(payment.Id.Value, provisioningRecord.PaymentId);
        Assert.False(provisioningRecord.UseTrial);
    }

    [Fact]
    public async Task VerifyPayment_Success_ActivatesSubscription()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var registerHandler = new RegisterTenantCommandHandler(provisioningService);

        var registerCommand = new RegisterTenantCommand(
            "Cafe Ferdowsi",
            "billing@eazymenu.ir",
            "+989121111222",
            "starter",
            "Mashhad",
            "Emam Reza St",
            "1234567890",
            UseTrial: false,
            DiscountCode: "WELCOME10");

        var registerResult = await registerHandler.HandleAsync(registerCommand);
        Assert.NotNull(registerResult.Payment);

        var verifyHandler = new VerifyPaymentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>(),
            scope.ServiceProvider.GetRequiredService<ITenantRepository>(),
            scope.ServiceProvider.GetRequiredService<IPaymentGatewayClient>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            scope.ServiceProvider.GetRequiredService<IDateTimeProvider>());

        var verifyResult = await verifyHandler.HandleAsync(new VerifyPaymentCommand(
            registerResult.Payment!.PaymentId.Value,
            registerResult.Payment.GatewayAuthority!));

        Assert.True(verifyResult.IsSuccessful);
        Assert.Equal(PaymentStatus.Succeeded, verifyResult.PaymentStatus);
        Assert.False(string.IsNullOrWhiteSpace(verifyResult.ReferenceCode));

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();

        var paymentRecord = await dbContext.PaymentTransactions.SingleAsync(p => p.Id == registerResult.Payment.PaymentId);
        Assert.Equal(PaymentStatus.Succeeded, paymentRecord.Status);
        Assert.NotNull(paymentRecord.ExternalReference);

        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .SingleAsync(t => t.Id == registerResult.TenantId);

        Assert.NotNull(tenant.ActiveSubscription);
        Assert.Equal(SubscriptionStatus.Active, tenant.ActiveSubscription!.Status);
        Assert.Equal(registerResult.SubscriptionId, tenant.ActiveSubscription.Id);
    }

    [Fact]
    public async Task VerifyPayment_Failure_KeepsSubscriptionPending()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var registerHandler = new RegisterTenantCommandHandler(provisioningService);

        var registerCommand = new RegisterTenantCommand(
            "Cafe Hafez",
            "billing@eazymenu.ir",
            "+989321234567",
            "starter",
            "Shiraz",
            "Hafez St",
            "9876543210",
            UseTrial: false,
            DiscountCode: "WELCOME10");

        var registerResult = await registerHandler.HandleAsync(registerCommand);
        Assert.NotNull(registerResult.Payment);

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var paymentTransaction = await paymentRepository.GetByIdAsync(registerResult.Payment!.PaymentId, default);
        Assert.NotNull(paymentTransaction);

        var failureAuthority = $"SANDBOX-FAIL-{Guid.NewGuid():N}";
        paymentTransaction!.AttachGatewayAuthority(failureAuthority);
        await paymentRepository.UpdateAsync(paymentTransaction, default);
        await unitOfWork.SaveChangesAsync();

        var verifyHandler = new VerifyPaymentCommandHandler(
            paymentRepository,
            scope.ServiceProvider.GetRequiredService<ITenantRepository>(),
            scope.ServiceProvider.GetRequiredService<IPaymentGatewayClient>(),
            unitOfWork,
            scope.ServiceProvider.GetRequiredService<IDateTimeProvider>());

        var verifyResult = await verifyHandler.HandleAsync(new VerifyPaymentCommand(
            registerResult.Payment.PaymentId.Value,
            failureAuthority));

        Assert.False(verifyResult.IsSuccessful);
        Assert.Equal(PaymentStatus.Failed, verifyResult.PaymentStatus);
        Assert.NotNull(verifyResult.FailureReason);

        var paymentRecord = await dbContext.PaymentTransactions.SingleAsync(p => p.Id == registerResult.Payment.PaymentId);
        Assert.Equal(PaymentStatus.Failed, paymentRecord.Status);
        Assert.Equal(failureAuthority, paymentRecord.GatewayAuthority);

        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .SingleAsync(t => t.Id == registerResult.TenantId);

        Assert.Null(tenant.ActiveSubscription);
        var pendingSubscription = Assert.Single(tenant.Subscriptions);
        Assert.Equal(SubscriptionStatus.Pending, pendingSubscription.Status);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddInfrastructureServices(configuration, options =>
        {
            options.UseInMemoryDatabase($"RegisterTenant-{Guid.NewGuid()}");
        });

        return services.BuildServiceProvider();
    }
}
