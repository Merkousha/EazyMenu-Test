using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Tenants.Branches.Tables.AddTable;
using EazyMenu.Application.Features.Tenants.Branches.Tables.RemoveTable;
using EazyMenu.Application.Features.Tenants.Branches.Tables.RestoreService;
using EazyMenu.Application.Features.Tenants.Branches.Tables.SetOutOfService;
using EazyMenu.Application.Features.Tenants.Branches.Tables.UpdateTable;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Branches;

public class BranchTableCommandHandlerTests
{
    [Fact]
    public async Task AddBranchTable_ShouldPersistTable()
    {
        var setup = TestSetup.Create();
        var handler = new AddBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var command = new AddBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            "VIP-1",
            Capacity: 4,
            IsOutdoor: false);

        var tableId = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Contains(setup.Branch.Tables, table => table.Id == tableId);
        Assert.True(setup.BranchRepository.WasUpdated);
    }

    [Fact]
    public async Task UpdateBranchTable_ShouldModifyFields()
    {
        var setup = TestSetup.Create();
        var addHandler = new AddBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var tableId = await addHandler.HandleAsync(new AddBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            "Hall-1",
            4,
            false), CancellationToken.None);

        var handler = new UpdateBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var command = new UpdateBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            tableId.Value,
            Label: "Hall-2",
            Capacity: 6,
            IsOutdoor: true);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        var table = setup.Branch.Tables.Single(t => t.Id == tableId);
        Assert.Equal("Hall-2", table.Label);
        Assert.Equal(6, table.Capacity);
        Assert.True(table.IsOutdoor);
    }

    [Fact]
    public async Task RemoveBranchTable_ShouldDeleteTable()
    {
        var setup = TestSetup.Create();
        var addHandler = new AddBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var tableId = await addHandler.HandleAsync(new AddBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            "Temporary",
            2,
            false), CancellationToken.None);

        var handler = new RemoveBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var result = await handler.HandleAsync(new RemoveBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            tableId.Value), CancellationToken.None);

        Assert.True(result);
        Assert.DoesNotContain(setup.Branch.Tables, table => table.Id == tableId);
    }

    [Fact]
    public async Task MarkBranchTableOutOfService_ShouldFlagTable()
    {
        var setup = TestSetup.Create();
        var addHandler = new AddBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var tableId = await addHandler.HandleAsync(new AddBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            "Outdoor",
            4,
            true), CancellationToken.None);

        var handler = new MarkBranchTableOutOfServiceCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        await handler.HandleAsync(new MarkBranchTableOutOfServiceCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            tableId.Value), CancellationToken.None);

        Assert.True(setup.Branch.Tables.Single(t => t.Id == tableId).IsOutOfService);
    }

    [Fact]
    public async Task RestoreBranchTableToService_ShouldRemoveOutOfServiceFlag()
    {
        var setup = TestSetup.Create();
        var addHandler = new AddBranchTableCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        var tableId = await addHandler.HandleAsync(new AddBranchTableCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            "Outdoor",
            4,
            true), CancellationToken.None);

        var markHandler = new MarkBranchTableOutOfServiceCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        await markHandler.HandleAsync(new MarkBranchTableOutOfServiceCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            tableId.Value), CancellationToken.None);

        var handler = new RestoreBranchTableToServiceCommandHandler(setup.BranchRepository, setup.UnitOfWork);
        await handler.HandleAsync(new RestoreBranchTableToServiceCommand(
            setup.TenantId.Value,
            setup.Branch.Id.Value,
            tableId.Value), CancellationToken.None);

        Assert.False(setup.Branch.Tables.Single(t => t.Id == tableId).IsOutOfService);
    }

    private sealed class TestSetup
    {
        public Tenant Tenant { get; }
        public Branch Branch { get; }
        public TenantId TenantId => Tenant.Id;
        public InMemoryBranchRepository BranchRepository { get; }
        public FakeUnitOfWork UnitOfWork { get; }

        private TestSetup(Tenant tenant, Branch branch, InMemoryBranchRepository branchRepository, FakeUnitOfWork unitOfWork)
        {
            Tenant = tenant;
            Branch = branch;
            BranchRepository = branchRepository;
            UnitOfWork = unitOfWork;
        }

        public static TestSetup Create()
        {
            var brand = BrandProfile.Create("Cafe");
            var email = Email.Create("owner@example.com");
            var phone = PhoneNumber.Create("+989121234567");
            var tenant = Tenant.Register("کافه نمونه", brand, email, phone, Address.Create("تهران", "خیابان انقلاب", "00000"));
            var branch = tenant.AddBranch("شعبه مرکزی", Address.Create("تهران", "خیابان مرکزی", "11111"));

            var repository = new InMemoryBranchRepository();
            repository.Seed(tenant.Id, branch);
            var unitOfWork = new FakeUnitOfWork();

            return new TestSetup(tenant, branch, repository, unitOfWork);
        }
    }

    private sealed class InMemoryBranchRepository : IBranchRepository
    {
        private readonly ConcurrentDictionary<(TenantId TenantId, BranchId BranchId), Branch> _branches = new();

        public bool WasUpdated { get; private set; }

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
            WasUpdated = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
