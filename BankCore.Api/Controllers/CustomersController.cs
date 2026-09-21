using BankCore.Api.Auth;
using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly BankDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public CustomersController(BankDbContext context)
    {
        _context = context;
    }

    // Kayıt herkese açık olmalı: henüz token'ı olmayan biri müşteri olabilmeli.
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            return Conflict("Bu e-posta adresi zaten kayıtlı.");

        if (await _context.Customers.AnyAsync(c => c.IdentityNumber == request.IdentityNumber))
            return Conflict("Bu kimlik numarası zaten kayıtlı.");

        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            IdentityNumber = request.IdentityNumber,
        };
        customer.PasswordHash = _passwordHasher.HashPassword(customer, request.Password);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ToResponse(customer));
    }

    // Sadece Admin (banka yetkilisi) tüm müşteri listesini görebilir.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CustomerResponse>>> GetAll()
    {
        var customers = await _context.Customers.ToListAsync();
        return customers.Select(ToResponse).ToList();
    }

    // Bir müşteri sadece kendi profilini görebilir; Admin herkesinkini görebilir.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id)
    {
        if (id != User.GetCustomerId() && !User.IsAdmin())
            return Forbid();

        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return ToResponse(customer);
    }

    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Role, c.CreatedAt);
}
