using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.SetOutOfService;

public sealed class MarkBranchTableOutOfServiceCommandHandler : ICommandHandler<MarkBranchTableOutOfServiceCommand, bool>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkBranchTableOutOfServiceCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(MarkBranchTableOutOfServiceCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(command.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        if (!TableId.TryCreate(command.TableId, out var tableId))
        {
            throw new BusinessRuleViolationException("شناسه میز معتبر نیست.");
        }

        var branch = await _branchRepository.GetByIdAsync(tenantId, branchId, cancellationToken);
        if (branch is null)
        {
            throw new NotFoundException("شعبه مورد نظر یافت نشد.");
        }

        branch.MarkTableOutOfService(tableId);

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
