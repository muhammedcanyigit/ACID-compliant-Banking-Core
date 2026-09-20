namespace BankCore.Api.Services;

public interface IRefreshTokenStore
{
    Task StoreAsync(string refreshToken, Guid customerId, TimeSpan ttl);
    Task<Guid?> GetCustomerIdAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}
