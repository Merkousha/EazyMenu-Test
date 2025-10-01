using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Identity;

public sealed class UserRepository : IUserRepository
{
    private readonly EazyMenuDbContext _context;

    public UserRepository(EazyMenuDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<List<User>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }
}
