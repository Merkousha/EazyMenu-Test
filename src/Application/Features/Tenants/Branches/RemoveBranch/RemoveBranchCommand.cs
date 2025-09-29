using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.RemoveBranch;

public sealed record RemoveBranchCommand(Guid TenantId, Guid BranchId) : ICommand<bool>;
