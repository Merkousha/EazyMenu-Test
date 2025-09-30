using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence;

public sealed class EazyMenuDbContext : DbContext, IUnitOfWork
{
    public EazyMenuDbContext(DbContextOptions<EazyMenuDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public new Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EazyMenuDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
