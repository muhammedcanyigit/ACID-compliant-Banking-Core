using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using BankCore.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly BankDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher = new();
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IConfiguration _configuration;

    public AuthController(
        BankDbContext context,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokenStore,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
        if (customer is null)
            return Unauthorized("E-posta veya şifre hatalı.");

        var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("E-posta veya şifre hatalı.");

        return await IssueTokensAsync(customer);
    }

    // Refresh token rotasyonu: gelen token bir kereliğine kullanılır, hemen iptal edilip
    // yerine yenisi verilir. Böylece bir token çalınsa bile sonsuza kadar geçerli kalmaz.
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var customerId = await _refreshTokenStore.GetCustomerIdAsync(request.RefreshToken);
        if (customerId is null)
            return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");

        await _refreshTokenStore.RevokeAsync(request.RefreshToken);

        var customer = await _context.Customers.FindAsync(customerId.Value);
        if (customer is null)
            return Unauthorized();

        return await IssueTokensAsync(customer);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        await _refreshTokenStore.RevokeAsync(request.RefreshToken);
        return NoContent();
    }

    private async Task<AuthResponse> IssueTokensAsync(Customer customer)
    {
        var accessToken = _tokenService.GenerateAccessToken(customer);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshDays = double.Parse(_configuration["Jwt:RefreshTokenDays"]!);
        await _refreshTokenStore.StoreAsync(refreshToken, customer.Id, TimeSpan.FromDays(refreshDays));

        var accessMinutes = double.Parse(_configuration["Jwt:AccessTokenMinutes"]!);
        return new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(accessMinutes));
    }
}
