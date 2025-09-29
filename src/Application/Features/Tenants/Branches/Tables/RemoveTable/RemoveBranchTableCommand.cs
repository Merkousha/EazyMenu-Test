using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.RemoveTable;

public sealed record RemoveBranchTableCommand(Guid TenantId, Guid BranchId, Guid TableId) : ICommand<bool>;
