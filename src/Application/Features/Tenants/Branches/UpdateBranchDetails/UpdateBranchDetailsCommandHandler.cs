using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.UpdateBranchDetails;

public sealed class UpdateBranchDetailsCommandHandler : ICommandHandler<UpdateBranchDetailsCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchDetailsCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(UpdateBranchDetailsCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(command.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new NotFoundException("مستاجر مورد نظر یافت نشد.");
        }

        var branch = tenant.Branches.SingleOrDefault(b => b.Id == branchId);
        if (branch is null)
        {
            throw new NotFoundException("شعبه مورد نظر یافت نشد.");
        }

        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            branch.UpdateName(command.Name);
        }

        var hasAnyAddressField = command.City is not null || command.Street is not null || command.PostalCode is not null;
        if (hasAnyAddressField)
        {
            if (string.IsNullOrWhiteSpace(command.City) || string.IsNullOrWhiteSpace(command.Street) || string.IsNullOrWhiteSpace(command.PostalCode))
            {
                throw new BusinessRuleViolationException("برای به‌روزرسانی آدرس باید هر سه فیلد شهر، خیابان و کدپستی تکمیل شوند.");
            }

            var address = Address.Create(command.City, command.Street, command.PostalCode);
            branch.UpdateAddress(address);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
