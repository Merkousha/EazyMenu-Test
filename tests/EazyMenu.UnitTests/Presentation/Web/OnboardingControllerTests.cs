using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Web.Controllers;
using EazyMenu.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;

namespace EazyMenu.UnitTests.Presentation.Web;

public sealed class OnboardingControllerTests
{
    [Fact]
    public void Start_Get_ReturnsViewWithDefaultModel()
    {
        var controller = CreateController(new StubRegisterTenantHandler(_ => throw new InvalidOperationException()));

        var result = controller.Start();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RegisterTenantViewModel>(viewResult.Model);
        Assert.True(model.UseTrial);
        Assert.NotEmpty(model.Plans);
        Assert.Equal("starter", model.PlanCode);
    }

    [Fact]
    public async Task Start_Post_InvalidModel_ReturnsViewWithErrors()
    {
        var controller = CreateController(new StubRegisterTenantHandler(_ => throw new InvalidOperationException()));
        controller.ModelState.AddModelError("RestaurantName", "Required");

        var model = new RegisterTenantViewModel
        {
            PlanCode = "starter",
            Plans = Array.Empty<SelectListItem>()
        };

        var result = await controller.Start(model, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Start_Post_WithTrial_ReturnsSuccessView()
    {
        var tenantId = TenantId.New();
        var subscriptionId = Guid.NewGuid();
        var controller = CreateController(new StubRegisterTenantHandler(_ => new TenantProvisioningResult(
            tenantId,
            subscriptionId,
            null)));

        var model = new RegisterTenantViewModel
        {
            RestaurantName = "Cafe Tehran",
            ManagerEmail = "owner@example.com",
            ManagerPhone = "+989121234567",
            PlanCode = "starter",
            City = "Tehran",
            Street = "Valiasr Ave",
            PostalCode = "1966731111",
            UseTrial = true,
            Plans = Array.Empty<SelectListItem>()
        };

        var result = await controller.Start(model, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Success", viewResult.ViewName);
        var successModel = Assert.IsType<ProvisioningSuccessViewModel>(viewResult.Model);
        Assert.Equal(tenantId.Value.ToString(), successModel.TenantId);
        Assert.Equal(subscriptionId.ToString(), successModel.SubscriptionId);
    }

    [Fact]
    public async Task Start_Post_WithPaymentRedirect_ReturnsRedirect()
    {
        var tenantId = TenantId.New();
        var subscriptionId = Guid.NewGuid();
        var paymentId = PaymentId.New();
        var redirectUri = new Uri("https://sandbox.zarinpal.com/pay/123456");

        var controller = CreateController(new StubRegisterTenantHandler(_ => new TenantProvisioningResult(
            tenantId,
            subscriptionId,
            new ProvisionedPayment(paymentId, PaymentStatus.Pending, redirectUri, "AUTH123"))));

        var model = new RegisterTenantViewModel
        {
            RestaurantName = "Cafe Tehran",
            ManagerEmail = "owner@example.com",
            ManagerPhone = "+989121234567",
            PlanCode = "starter",
            City = "Tehran",
            Street = "Valiasr Ave",
            PostalCode = "1966731111",
            UseTrial = false,
            Plans = Array.Empty<SelectListItem>()
        };

        var result = await controller.Start(model, CancellationToken.None);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(redirectUri.AbsoluteUri, redirect.Url);
        Assert.Equal(tenantId.Value.ToString(), controller.TempData["OnboardingTenantId"]);
        Assert.Equal(subscriptionId.ToString(), controller.TempData["OnboardingSubscriptionId"]);
    }

    private static OnboardingController CreateController(ICommandHandler<RegisterTenantCommand, TenantProvisioningResult> handler)
    {
        var controller = new OnboardingController(handler, NullLogger<OnboardingController>.Instance)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new InMemoryTempDataProvider())
        };

        return controller;
    }

    private sealed class StubRegisterTenantHandler : ICommandHandler<RegisterTenantCommand, TenantProvisioningResult>
    {
        private readonly Func<RegisterTenantCommand, TenantProvisioningResult> _factory;

        public StubRegisterTenantHandler(Func<RegisterTenantCommand, TenantProvisioningResult> factory)
        {
            _factory = factory;
        }

        public Task<TenantProvisioningResult> HandleAsync(RegisterTenantCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_factory(command));
        }
    }

    private sealed class InMemoryTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) => new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
        }
    }
}
