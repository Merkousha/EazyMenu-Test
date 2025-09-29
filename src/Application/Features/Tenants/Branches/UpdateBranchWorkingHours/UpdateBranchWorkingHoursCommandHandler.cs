using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.UpdateBranchWorkingHours;

public sealed class UpdateBranchWorkingHoursCommandHandler : ICommandHandler<UpdateBranchWorkingHoursCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchWorkingHoursCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(UpdateBranchWorkingHoursCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(command.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        if (command.WorkingHours is null)
        {
            throw new BusinessRuleViolationException("لیست ساعات کاری ارسال نشده است.");
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

        var scheduleSlots = command.WorkingHours
            .Select(ToScheduleSlot)
            .ToList();

        branch.UpdateWorkingHours(scheduleSlots);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ScheduleSlot ToScheduleSlot(WorkingHourDto dto)
    {
        return ScheduleSlot.Create(dto.DayOfWeek, dto.Start, dto.End);
    }
}
