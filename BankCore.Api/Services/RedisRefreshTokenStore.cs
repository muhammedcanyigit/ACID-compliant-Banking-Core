using Microsoft.Extensions.Caching.Distributed;

namespace BankCore.Api.Services;

// Refresh token'ları Redis'te "refresh-token:{token}" -> customerId şeklinde,
// süresi dolunca kendiliğinden silinecek (TTL) kayıtlar olarak tutar.
public class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly IDistributedCache _cache;

    public RedisRefreshTokenStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    private static string Key(string refreshToken) => $"refresh-token:{refreshToken}";

    public Task StoreAsync(string refreshToken, Guid customerId, TimeSpan ttl) =>
        _cache.SetStringAsync(
            Key(refreshToken),
            customerId.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

    public async Task<Guid?> GetCustomerIdAsync(string refreshToken)
    {
        var value = await _cache.GetStringAsync(Key(refreshToken));
        return value is null ? null : Guid.Parse(value);
    }

    public Task RevokeAsync(string refreshToken) => _cache.RemoveAsync(Key(refreshToken));
}
