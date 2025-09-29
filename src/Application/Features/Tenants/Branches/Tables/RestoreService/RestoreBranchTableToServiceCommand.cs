using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.RestoreService;

public sealed record RestoreBranchTableToServiceCommand(Guid TenantId, Guid BranchId, Guid TableId) : ICommand<bool>;
