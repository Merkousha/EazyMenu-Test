using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Security;
using Microsoft.Extensions.Caching.Memory;

namespace EazyMenu.Infrastructure.Security;

internal sealed class InMemoryOneTimePasswordStore : IOneTimePasswordStore
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDateTimeProvider _dateTimeProvider;

    public InMemoryOneTimePasswordStore(IMemoryCache memoryCache, IDateTimeProvider dateTimeProvider)
    {
        _memoryCache = memoryCache;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task StoreAsync(string phoneNumber, string code, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        var cacheEntry = new OtpEntry(code, expiresAtUtc);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiresAtUtc
        };

        _memoryCache.Set(BuildKey(phoneNumber), cacheEntry, cacheOptions);
        return Task.CompletedTask;
    }

    public Task<OneTimePasswordValidationResult> ValidateAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        if (!_memoryCache.TryGetValue(BuildKey(phoneNumber), out OtpEntry? entry) || entry is null)
        {
            return Task.FromResult(OneTimePasswordValidationResult.Invalid("کد ارسال شده یافت نشد یا منقضی شده است."));
        }

        if (_dateTimeProvider.UtcNow >= entry.ExpiresAtUtc)
        {
            _memoryCache.Remove(BuildKey(phoneNumber));
            return Task.FromResult(OneTimePasswordValidationResult.Expired("کد ارسال شده منقضی شده است."));
        }

        if (!string.Equals(entry.Code, code, StringComparison.Ordinal))
        {
            return Task.FromResult(OneTimePasswordValidationResult.Invalid("کد وارد شده صحیح نیست."));
        }

        _memoryCache.Remove(BuildKey(phoneNumber));
        return Task.FromResult(OneTimePasswordValidationResult.Success());
    }

    private static string BuildKey(string phoneNumber) => $"otp:{phoneNumber}";

    private sealed record OtpEntry(string Code, DateTime ExpiresAtUtc);
}
