using BankCore.Api.Entities;

namespace BankCore.Api.Services;

public interface ITokenService
{
    string GenerateAccessToken(Customer customer);
    string GenerateRefreshToken();
}
