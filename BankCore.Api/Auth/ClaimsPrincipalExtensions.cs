using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankCore.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    // Token'ın "sub" claim'inden isteği yapan müşterinin kimliğini çıkarır.
    public static Guid GetCustomerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.Parse(value!);
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
}
