using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Identity.Login;

namespace EazyMenu.Application.Features.Identity.Queries.GetUsersByTenant;

/// <summary>
/// کوئری دریافت لیست کاربران یک مستاجر.
/// </summary>
public sealed record GetUsersByTenantQuery(Guid TenantId) : IQuery<List<UserDto>>;
