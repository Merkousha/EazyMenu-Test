using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Tenants.Branches.GetBranchDetails;
using EazyMenu.Application.Features.Tenants.Branches.GetTenantBranches;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Branches;

public class TenantBranchQueryHandlerTests
{
    [Fact]
    public async Task GetTenantBranches_ShouldReturnSummariesOrderedByName()
    {
        var tenant = CreateTenant();
        tenant.AddBranch("زودتر", Address.Create("تهران", "خیابان 1", "11111"));
        tenant.AddBranch("آخر", Address.Create("اصفهان", "خیابان 2", "22222"));

        var repository = new InMemoryTenantRepository(tenant);
        var handler = new GetTenantBranchesQueryHandler(repository);
        var result = await handler.HandleAsync(new GetTenantBranchesQuery(tenant.Id.Value), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "آخر", "زودتر" }, result.Select(r => r.Name));
    }

    [Fact]
    public async Task GetBranchDetails_ShouldReturnFullSnapshot()
    {
        var tenant = CreateTenant();
        var branch = tenant.AddBranch("مرکزی", Address.Create("تهران", "خیابان اصلی", "33333"));
        branch.UpdateWorkingHours(new List<ScheduleSlot>
        {
            ScheduleSlot.Create(DayOfWeek.Saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
        });
        var table = branch.AddTable("VIP", 4, true);
        branch.MarkTableOutOfService(table.Id);

        var repository = new InMemoryBranchRepository();
        repository.Seed(tenant.Id, branch);

        var handler = new GetBranchDetailsQueryHandler(repository);
        var dto = await handler.HandleAsync(new GetBranchDetailsQuery(tenant.Id.Value, branch.Id.Value), CancellationToken.None);

        Assert.Equal(branch.Id.Value, dto.BranchId);
        Assert.Single(dto.WorkingHours);
        Assert.Single(dto.Tables);
        var tableDto = dto.Tables.Single();
        Assert.Equal(table.Id.Value, tableDto.TableId);
        Assert.True(tableDto.IsOutOfService);
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
        private readonly Tenant _tenant;

        public InMemoryTenantRepository(Tenant tenant)
        {
            _tenant = tenant;
        }

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Tenant?>(_tenant.Id == tenantId ? _tenant : null);
        }

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tenant.Slug == slug ? _tenant : null);
        }
    }

    private sealed class InMemoryBranchRepository : IBranchRepository
    {
        private readonly Dictionary<(TenantId TenantId, BranchId BranchId), Branch> _branches = new();

        public void Seed(TenantId tenantId, Branch branch)
        {
            _branches[(tenantId, branch.Id)] = branch;
        }

        public Task<Branch?> GetByIdAsync(TenantId tenantId, BranchId branchId, CancellationToken cancellationToken = default)
        {
            _branches.TryGetValue((tenantId, branchId), out var branch);
            return Task.FromResult(branch);
        }

        public Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
