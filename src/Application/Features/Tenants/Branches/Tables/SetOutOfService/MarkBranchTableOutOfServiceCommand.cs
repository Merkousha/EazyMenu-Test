using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.SetOutOfService;

public sealed record MarkBranchTableOutOfServiceCommand(Guid TenantId, Guid BranchId, Guid TableId) : ICommand<bool>;
