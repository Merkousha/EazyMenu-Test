using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Common.Time;
using EazyMenu.Application.Features.Customers.Login;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Commands.AddMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.AddMenuItem;
using EazyMenu.Application.Features.Menus.Commands.AdjustMenuItemInventory;
using EazyMenu.Application.Features.Menus.Commands.ArchiveMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.CreateMenu;
using EazyMenu.Application.Features.Menus.Commands.PublishMenu;
using EazyMenu.Application.Features.Menus.Commands.QuickUpdateMenuItem;
using EazyMenu.Application.Features.Menus.Commands.RemoveMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.RemoveMenuItem;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuCategories;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuItems;
using EazyMenu.Application.Features.Menus.Commands.RestoreMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.SetMenuItemAvailability;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemDetails;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemPricing;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuMetadata;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Application.Features.Menus.Queries.GetPublishedMenu;
using EazyMenu.Application.Features.Menus.Queries.GetMenus;
using EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;
using EazyMenu.Application.Features.Notifications.GetSmsUsageSummary;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Application.Features.Orders.CancelOrder;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Application.Features.Orders.ConfirmOrder;
using EazyMenu.Application.Features.Orders.CompleteOrder;
using EazyMenu.Application.Features.Orders.GetOrderDetails;
using EazyMenu.Application.Features.Orders.GetOrders;
using EazyMenu.Application.Features.Orders.PlaceOrder;
using EazyMenu.Application.Features.Payments.VerifyPayment;
using EazyMenu.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace EazyMenu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: register MediatR, validators, and mapping profiles when implemented.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICommandHandler<RegisterTenantCommand, TenantProvisioningResult>, RegisterTenantCommandHandler>();
        services.AddScoped<ICommandHandler<RequestCustomerLoginCommand, RequestCustomerLoginResult>, RequestCustomerLoginCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyCustomerLoginCommand, VerifyCustomerLoginResult>, VerifyCustomerLoginCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult>, VerifyPaymentCommandHandler>();
        services.AddScoped<ICommandHandler<CreateMenuCommand, Guid>, CreateMenuCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuMetadataCommand, bool>, UpdateMenuMetadataCommandHandler>();
        services.AddScoped<ICommandHandler<AddMenuCategoryCommand, Guid>, AddMenuCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuCategoryCommand, bool>, UpdateMenuCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveMenuCategoryCommand, bool>, ArchiveMenuCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<RestoreMenuCategoryCommand, bool>, RestoreMenuCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMenuCategoryCommand, bool>, RemoveMenuCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<ReorderMenuCategoriesCommand, bool>, ReorderMenuCategoriesCommandHandler>();
        services.AddScoped<ICommandHandler<AddMenuItemCommand, Guid>, AddMenuItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuItemDetailsCommand, bool>, UpdateMenuItemDetailsCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuItemPricingCommand, bool>, UpdateMenuItemPricingCommandHandler>();
        services.AddScoped<ICommandHandler<SetMenuItemAvailabilityCommand, bool>, SetMenuItemAvailabilityCommandHandler>();
        services.AddScoped<ICommandHandler<QuickUpdateMenuItemCommand, bool>, QuickUpdateMenuItemCommandHandler>();
        services.AddScoped<ICommandHandler<AdjustMenuItemInventoryCommand, bool>, AdjustMenuItemInventoryCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMenuItemCommand, bool>, RemoveMenuItemCommandHandler>();
        services.AddScoped<ICommandHandler<ReorderMenuItemsCommand, bool>, ReorderMenuItemsCommandHandler>();
        services.AddScoped<ICommandHandler<PublishMenuCommand, int>, PublishMenuCommandHandler>();
        services.AddScoped<IQueryHandler<GetMenusQuery, IReadOnlyCollection<MenuSummaryDto>>, GetMenusQueryHandler>();
        services.AddScoped<IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto>, GetMenuDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPublishedMenuQuery, PublishedMenuDto?>, GetPublishedMenuQueryHandler>();
        services.AddScoped<IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage>, GetSmsDeliveryLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSmsUsageSummaryQuery, SmsUsageSummary>, GetSmsUsageSummaryQueryHandler>();
        
        // Order commands and queries
        services.AddScoped<ICommandHandler<PlaceOrderCommand, OrderId>, PlaceOrderCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmOrderCommand, bool>, ConfirmOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteOrderCommand, bool>, CompleteOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelOrderCommand, bool>, CancelOrderCommandHandler>();
        services.AddScoped<IQueryHandler<GetOrdersQuery, IReadOnlyCollection<OrderSummaryDto>>, GetOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>, GetOrderDetailsQueryHandler>();
        
        return services;
    }
}
