using System;
using System.Collections.Generic;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Aggregates.Users.Events;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Users;

/// <summary>
/// کاربر سیستم که به یک مستاجر (رستوران) تعلق دارد.
/// </summary>
public sealed class User : Entity<UserId>, IAggregateRoot
{
    // Private parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private User() : base(default)
    {
        // Required by EF Core
    }
#pragma warning restore CS8618

    private User(
        UserId id,
        TenantId tenantId,
        string email,
        string fullName,
        PhoneNumber phoneNumber,
        PasswordHash passwordHash,
        UserRole role,
        UserStatus status,
        DateTime createdAtUtc,
        DateTime? lastLoginAtUtc) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        Role = role;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        LastLoginAtUtc = lastLoginAtUtc;
    }
    
    /// <summary>
    /// شناسه مستاجر (رستوران) که کاربر به آن تعلق دارد.
    /// </summary>
    public TenantId TenantId { get; private set; }
    
    /// <summary>
    /// آدرس ایمیل کاربر (یکتا در سطح سیستم).
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// نام کامل کاربر.
    /// </summary>
    public string FullName { get; private set; }
    
    /// <summary>
    /// شماره تلفن کاربر.
    /// </summary>
    public PhoneNumber PhoneNumber { get; private set; }
    
    /// <summary>
    /// رمز عبور هش شده کاربر.
    /// </summary>
    public PasswordHash PasswordHash { get; private set; }
    
    /// <summary>
    /// نقش کاربر در سیستم.
    /// </summary>
    public UserRole Role { get; private set; }
    
    /// <summary>
    /// وضعیت کاربر.
    /// </summary>
    public UserStatus Status { get; private set; }
    
    /// <summary>
    /// تاریخ ایجاد کاربر.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }
    
    /// <summary>
    /// آخرین زمان ورود کاربر به سیستم.
    /// </summary>
    public DateTime? LastLoginAtUtc { get; private set; }
    
    /// <summary>
    /// ایجاد کاربر جدید.
    /// </summary>
    public static User Create(
        TenantId tenantId,
        string email,
        string fullName,
        PhoneNumber phoneNumber,
        PasswordHash passwordHash,
        UserRole role)
    {
        if (tenantId == default || tenantId.Value == Guid.Empty)
        {
            throw new DomainValidationException("شناسه مستاجر نامعتبر است.");
        }
        
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("ایمیل نمی‌تواند خالی باشد.");
        }
        
        if (!IsValidEmail(email))
        {
            throw new DomainValidationException("فرمت ایمیل نامعتبر است.");
        }
        
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException("نام کامل نمی‌تواند خالی باشد.");
        }
        
        if (fullName.Length < 2 || fullName.Length > 100)
        {
            throw new DomainValidationException("نام کامل باید بین ۲ تا ۱۰۰ کاراکتر باشد.");
        }
        
        var userId = UserId.New();
        var now = DateTime.UtcNow;
        
        var user = new User(
            userId,
            tenantId,
            email.ToLowerInvariant(),
            fullName.Trim(),
            phoneNumber,
            passwordHash,
            role,
            UserStatus.Active,
            now,
            null);
        
        user.RaiseDomainEvent(new UserCreatedDomainEvent(
            userId,
            tenantId,
            email,
            role,
            now));
        
        return user;
    }
    
    /// <summary>
    /// تغییر نقش کاربر.
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        if (Status != UserStatus.Active)
        {
            throw new BusinessRuleViolationException("فقط کاربران فعال می‌توانند نقش تغییر یابند.");
        }
        
        if (Role == newRole)
        {
            return; // No change needed
        }
        
        var oldRole = Role;
        Role = newRole;
        
        RaiseDomainEvent(new UserRoleChangedDomainEvent(
            Id,
            oldRole,
            newRole,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// تغییر رمز عبور کاربر.
    /// </summary>
    public void ChangePassword(PasswordHash newPasswordHash)
    {
        if (Status != UserStatus.Active)
        {
            throw new BusinessRuleViolationException("فقط کاربران فعال می‌توانند رمز عبور را تغییر دهند.");
        }
        
        PasswordHash = newPasswordHash ?? throw new DomainValidationException("رمز عبور جدید نمی‌تواند null باشد.");
        
        RaiseDomainEvent(new UserPasswordChangedDomainEvent(
            Id,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// به‌روزرسانی اطلاعات پروفایل کاربر.
    /// </summary>
    public void UpdateProfile(string fullName, PhoneNumber phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException("نام کامل نمی‌تواند خالی باشد.");
        }
        
        if (fullName.Length < 2 || fullName.Length > 100)
        {
            throw new DomainValidationException("نام کامل باید بین ۲ تا ۱۰۰ کاراکتر باشد.");
        }
        
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
    }
    
    /// <summary>
    /// ثبت آخرین زمان ورود کاربر.
    /// </summary>
    public void RecordLogin()
    {
        if (Status != UserStatus.Active)
        {
            throw new BusinessRuleViolationException("کاربر غیرفعال نمی‌تواند وارد سیستم شود.");
        }
        
        LastLoginAtUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// غیرفعال کردن کاربر.
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
        {
            return; // Already inactive
        }
        
        Status = UserStatus.Inactive;
    }
    
    /// <summary>
    /// فعال کردن مجدد کاربر.
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
        {
            return; // Already active
        }
        
        if (Status == UserStatus.Blocked)
        {
            throw new BusinessRuleViolationException("کاربر مسدود شده نمی‌تواند به صورت خودکار فعال شود.");
        }
        
        Status = UserStatus.Active;
    }
    
    /// <summary>
    /// مسدود کردن کاربر.
    /// </summary>
    public void Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("دلیل مسدود کردن باید مشخص شود.");
        }
        
        Status = UserStatus.Blocked;
    }
    
    /// <summary>
    /// بررسی آیا کاربر می‌تواند وارد سیستم شود.
    /// </summary>
    public bool CanLogin() => Status == UserStatus.Active;
    
    /// <summary>
    /// بررسی معتبر بودن ایمیل.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
