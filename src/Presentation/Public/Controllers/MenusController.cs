using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetPublishedMenu;
using EazyMenu.Public.Models.Menus;
using EazyMenu.Public.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EazyMenu.Public.Controllers;

public sealed class MenusController : Controller
{
    private readonly ILogger<MenusController> _logger;
    private readonly IQueryHandler<GetPublishedMenuQuery, PublishedMenuDto?> _publishedMenuQueryHandler;
    private readonly TenantSiteOptions _tenantSiteOptions;

    public MenusController(
        ILogger<MenusController> logger,
        IQueryHandler<GetPublishedMenuQuery, PublishedMenuDto?> publishedMenuQueryHandler,
        IOptions<TenantSiteOptions> tenantSiteOptions)
    {
        _logger = logger;
        _publishedMenuQueryHandler = publishedMenuQueryHandler;
        _tenantSiteOptions = tenantSiteOptions.Value;
    }

    [HttpGet("menus")]
    public IActionResult Index([FromQuery(Name = "q")] string? query)
    {
        if (_tenantSiteOptions.DefaultTenantId == Guid.Empty)
        {
            return View("Index", new PublicMenuPageViewModel(Guid.Empty, false, null, NormalizeQuery(query), false));
        }

        return RedirectToAction(nameof(IndexByTenant), new { tenantId = _tenantSiteOptions.DefaultTenantId, q = NormalizeQuery(query) });
    }

    [HttpGet("menus/{tenantId:guid}")]
    public async Task<IActionResult> IndexByTenant(
        Guid tenantId,
        Guid? menuId,
        [FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        PublishedMenuDto? snapshot;
        try
        {
            snapshot = await _publishedMenuQueryHandler.HandleAsync(
                new GetPublishedMenuQuery(tenantId, menuId),
                cancellationToken);
        }
        catch (BusinessRuleViolationException exception)
        {
            _logger.LogWarning(exception, "درخواست نامعتبر برای مشاهده منوی منتشرشده");
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطای غیرمنتظره در دریافت منوی منتشرشده مستاجر {TenantId}", tenantId);
            return StatusCode(500, new { message = "در بازیابی منو خطایی رخ داد." });
        }

        var normalizedQuery = NormalizeQuery(query);
        var model = PublicMenuViewModelFactory.CreatePageModel(snapshot, tenantId, searchTerm: normalizedQuery);
        ViewData["Title"] = model.HasMenu && model.Menu is not null
            ? model.Menu.DisplayName
            : "منوی دیجیتال";

        return View("Index", model);
    }

    private static string? NormalizeQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        return query.Trim();
    }
}
