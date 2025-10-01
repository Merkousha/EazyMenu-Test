using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Queries.GetPublishedMenu;

public sealed record GetPublishedMenuQuery(Guid TenantId, Guid? MenuId = null) : IQuery<PublishedMenuDto?>;
