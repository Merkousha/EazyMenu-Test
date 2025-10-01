using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Identity;

/// <summary>
/// ریپازیتوری کاربران سیستم.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// دریافت کاربر بر اساس شناسه.
    /// </summary>
    Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// دریافت کاربر بر اساس ایمیل (یکتا در سطح سیستم).
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// دریافت لیست کاربران یک مستاجر.
    /// </summary>
    Task<List<User>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// بررسی وجود کاربر با ایمیل مشخص.
    /// </summary>
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// افزودن کاربر جدید.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// به‌روزرسانی کاربر موجود.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// حذف کاربر (soft delete).
    /// </summary>
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
