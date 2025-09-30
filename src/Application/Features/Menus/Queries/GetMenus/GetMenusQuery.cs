using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;
using System.Collections.Generic;

namespace EazyMenu.Application.Features.Menus.Queries.GetMenus;

public sealed record GetMenusQuery(Guid TenantId, bool IncludeArchived = false) : IQuery<IReadOnlyCollection<MenuSummaryDto>>;
