using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.AddTable;

public sealed class AddBranchTableCommandHandler : ICommandHandler<AddBranchTableCommand, TableId>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBranchTableCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TableId> HandleAsync(AddBranchTableCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(command.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        var branch = await _branchRepository.GetByIdAsync(tenantId, branchId, cancellationToken);
        if (branch is null)
        {
            throw new NotFoundException("شعبه مورد نظر یافت نشد.");
        }

        var table = branch.AddTable(command.Label, command.Capacity, command.IsOutdoor);

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return table.Id;
    }
}
