using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly BankDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public CustomersController(BankDbContext context)
    {
        _context = context;
    }

    [HttpPost]
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return ToResponse(customer);
    }

    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Role, c.CreatedAt);
}
