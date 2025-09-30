using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;

public sealed record GetMenuDetailsQuery(Guid TenantId, Guid MenuId, bool IncludeArchivedCategories = true) : IQuery<MenuDetailsDto>;
