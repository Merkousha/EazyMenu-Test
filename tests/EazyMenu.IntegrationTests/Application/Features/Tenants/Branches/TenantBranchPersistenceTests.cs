using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Tenants.Branches.CreateBranch;
using EazyMenu.Application.Features.Tenants.Branches.UpdateBranchWorkingHours;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EazyMenu.IntegrationTests.Application.Features.Tenants.Branches;

public sealed class TenantBranchPersistenceTests
{
    [Fact]
    public async Task CreateBranch_PersistsBranchInDatabase()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var tenant = CreateTenant();
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var command = new CreateBranchCommand(
            tenant.Id.Value,
            "شعبه ونک",
            "تهران",
            "خیابان ونک",
            "0098765432");

        var handler = new CreateBranchCommandHandler(tenantRepository, unitOfWork);
        var branchId = await handler.HandleAsync(command, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var persisted = await tenantRepository.GetByIdAsync(tenant.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Contains(persisted!.Branches, branch => branch.Id == branchId && branch.Name == "شعبه ونک");
    }

    [Fact]
    public async Task UpdateBranchWorkingHours_PersistsChanges()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var tenant = CreateTenant();
        var branch = tenant.AddBranch("ساعات کاری", Address.Create("تهران", "خیابان آزادی", "11111"));
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var newSlots = new List<WorkingHourDto>
        {
            new(DayOfWeek.Saturday, new TimeSpan(9, 0, 0), new TimeSpan(14, 0, 0)),
            new(DayOfWeek.Sunday, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0))
        };

        var handler = new UpdateBranchWorkingHoursCommandHandler(tenantRepository, unitOfWork);
        var command = new UpdateBranchWorkingHoursCommand(tenant.Id.Value, branch.Id.Value, newSlots);

        var result = await handler.HandleAsync(command, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var persisted = await tenantRepository.GetByIdAsync(tenant.Id, CancellationToken.None);
        var persistedBranch = persisted!.Branches.Single(b => b.Id == branch.Id);

        Assert.True(result);
        Assert.Equal(2, persistedBranch.WorkingHours.Count);
        Assert.Contains(persistedBranch.WorkingHours, slot => slot.DayOfWeek == DayOfWeek.Sunday);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddInfrastructureServices(configuration, options =>
        {
            options.UseInMemoryDatabase($"TenantBranchTests-{Guid.NewGuid()}");
        });

        return services.BuildServiceProvider();
    }

    private static Tenant CreateTenant()
    {
        var brand = BrandProfile.Create("کافه تست");
        var email = Email.Create("owner@test.ir");
        var phone = PhoneNumber.Create("+989121234567");
        var address = Address.Create("تهران", "خیابان انقلاب", "1234567890");
        return Tenant.Register("کافه تست", brand, email, phone, address);
    }
}
