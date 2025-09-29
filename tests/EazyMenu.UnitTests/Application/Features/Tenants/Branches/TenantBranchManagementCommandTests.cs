using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Branches.CreateBranch;
using EazyMenu.Application.Features.Tenants.Branches.RemoveBranch;
using EazyMenu.Application.Features.Tenants.Branches.UpdateBranchDetails;
using EazyMenu.Application.Features.Tenants.Branches.UpdateBranchWorkingHours;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Branches;

public class TenantBranchManagementCommandTests
{
    [Fact]
    public async Task CreateBranch_ShouldAddBranchToTenant()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new CreateBranchCommandHandler(repository, new FakeUnitOfWork());
        var command = new CreateBranchCommand(
            tenant.Id.Value,
            "شعبه فردوسی",
            "تهران",
            "خیابان فردوسی",
            "1234567890");

        var branchId = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Contains(repository.Tenant.Branches, branch => branch.Id == branchId);
        Assert.True(repository.WasUpdated);
    }

    [Fact]
    public async Task UpdateBranchDetails_ShouldModifyNameAndAddress()
    {
        var tenant = CreateTenant();
        var branch = tenant.AddBranch("قدیمی", Address.Create("تهران", "خیابان اول", "11111"));
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new UpdateBranchDetailsCommandHandler(repository, new FakeUnitOfWork());
        var command = new UpdateBranchDetailsCommand(
            tenant.Id.Value,
            branch.Id.Value,
            Name: "جدید",
            City: "اصفهان",
            Street: "خیابان دوم",
            PostalCode: "22222");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("جدید", branch.Name);
        Assert.Equal("اصفهان", branch.Address.City);
        Assert.Equal("خیابان دوم", branch.Address.Street);
        Assert.Equal("22222", branch.Address.PostalCode);
    }

    [Fact]
    public async Task UpdateBranchDetails_WithPartialAddress_ShouldThrow()
    {
        var tenant = CreateTenant();
        var branch = tenant.AddBranch("قدیمی", Address.Create("تهران", "خیابان اول", "11111"));
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new UpdateBranchDetailsCommandHandler(repository, new FakeUnitOfWork());
        var command = new UpdateBranchDetailsCommand(
            tenant.Id.Value,
            branch.Id.Value,
            Name: null,
            City: "اصفهان",
            Street: null,
            PostalCode: "22222");

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveBranch_ShouldDeleteBranch()
    {
        var tenant = CreateTenant();
        var branch = tenant.AddBranch("قابل حذف", Address.Create("تهران", "خیابان حذف", "55555"));
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new RemoveBranchCommandHandler(repository, new FakeUnitOfWork());
        var command = new RemoveBranchCommand(tenant.Id.Value, branch.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.DoesNotContain(repository.Tenant.Branches, b => b.Id == branch.Id);
    }

    [Fact]
    public async Task UpdateBranchWorkingHours_ShouldReplaceSlots()
    {
        var tenant = CreateTenant();
        var branch = tenant.AddBranch("ساعات کاری", Address.Create("تهران", "خیابان", "44444"));
        branch.UpdateWorkingHours(new[] { ScheduleSlot.Create(DayOfWeek.Saturday, new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0)) });

        var repository = new InMemoryTenantRepository(tenant);
        var handler = new UpdateBranchWorkingHoursCommandHandler(repository, new FakeUnitOfWork());
        var command = new UpdateBranchWorkingHoursCommand(
            tenant.Id.Value,
            branch.Id.Value,
            new List<WorkingHourDto>
            {
                new(DayOfWeek.Sunday, new TimeSpan(10, 0, 0), new TimeSpan(16, 0, 0)),
                new(DayOfWeek.Monday, new TimeSpan(12, 0, 0), new TimeSpan(20, 0, 0))
            });

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(2, branch.WorkingHours.Count);
        Assert.Contains(branch.WorkingHours, slot => slot.DayOfWeek == DayOfWeek.Monday);
    }

    private static Tenant CreateTenant()
    {
        var brand = BrandProfile.Create("Cafe");
        var email = Email.Create("owner@example.com");
        var phone = PhoneNumber.Create("+989121234567");
        return Tenant.Register("کافه نمونه", brand, email, phone, Address.Create("تهران", "خیابان انقلاب", "00000"));
    }

    private sealed class InMemoryTenantRepository : ITenantRepository
    {
        public Tenant Tenant { get; }

        public bool WasUpdated { get; private set; }

        public InMemoryTenantRepository(Tenant tenant)
        {
            Tenant = tenant;
        }

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Tenant?>(Tenant.Id == tenantId ? Tenant : null);
        }

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            WasUpdated = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
