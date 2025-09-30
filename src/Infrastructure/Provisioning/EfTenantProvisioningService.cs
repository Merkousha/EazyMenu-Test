using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Time;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Models;

namespace EazyMenu.Infrastructure.Provisioning;

internal sealed class EfTenantProvisioningService : ITenantProvisioningService
{
    private readonly EazyMenuDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EfTenantProvisioningService(EazyMenuDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TenantId> ProvisionAsync(TenantProvisioningRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = Tenant.Register(
            request.RestaurantName,
            BrandProfile.Create(request.RestaurantName),
            Email.Create(request.ManagerEmail),
            PhoneNumber.Create(request.ManagerPhone),
            request.HeadquartersAddress);

        var record = TenantProvisioningRecord.Create(
            tenant.Id.Value,
            request.RestaurantName,
            Slugify(request.RestaurantName),
            request.ManagerEmail,
            request.ManagerPhone,
            request.PlanCode,
            request.HeadquartersAddress.City,
            request.HeadquartersAddress.Street,
            request.HeadquartersAddress.PostalCode,
            request.UseTrial,
            request.DiscountCode,
            _dateTimeProvider.UtcNow);

        _dbContext.Tenants.Add(tenant);
        await _dbContext.TenantProvisionings.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }

    private static string Slugify(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
