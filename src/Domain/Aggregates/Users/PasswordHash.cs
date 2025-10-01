using System;
using EazyMenu.Domain.Common.Exceptions;

namespace EazyMenu.Domain.Aggregates.Users;

/// <summary>
/// رمز عبور هش شده با الگوریتم امن.
/// </summary>
public sealed record PasswordHash
{
    /// <summary>
    /// مقدار هش شده رمز عبور.
    /// </summary>
    public string HashedValue { get; }
    
    /// <summary>
    /// الگوریتم استفاده شده برای هش (مثلاً bcrypt، PBKDF2).
    /// </summary>
    public string Algorithm { get; }
    
    private PasswordHash(string hashedValue, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
        {
            throw new DomainValidationException("رمز عبور هش شده نمی‌تواند خالی باشد.");
        }
        
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            throw new DomainValidationException("الگوریتم هش نمی‌تواند خالی باشد.");
        }
        
        HashedValue = hashedValue;
        Algorithm = algorithm;
    }
    
    /// <summary>
    /// ایجاد PasswordHash با رمز هش شده و الگوریتم مشخص.
    /// </summary>
    public static PasswordHash Create(string hashedValue, string algorithm = "bcrypt")
    {
        return new PasswordHash(hashedValue, algorithm);
    }
    
    public override string ToString() => $"{Algorithm}:{HashedValue}";
}
