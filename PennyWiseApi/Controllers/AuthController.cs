using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennyWiseApi.Data;
using PennyWiseApi.Models.DTOs.Auth;
using PennyWiseApi.Models.Entities;
using PennyWiseApi.Services.Interfaces;
using PennyWiseApi.Services.Implementations;

namespace PennyWiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ApplicationDbContext db,
    IPasswordHasherService passwordHasher,
    JwtTokenService jwt)
    : ControllerBase
{

    // POST: api/auth/register

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already registered.");

        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already taken.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = passwordHasher.Hash(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        var now = DateTime.UtcNow;

        var defaultCats = new List<Category>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, Name = "Salary",     Type = CategoryType.Income,  CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = user.Id, Name = "Food",       Type = CategoryType.Expense, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = user.Id, Name = "Transport",  Type = CategoryType.Expense, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = user.Id, Name = "Shopping",   Type = CategoryType.Expense, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = user.Id, Name = "Bills",      Type = CategoryType.Expense, CreatedAt = now }
        };

        db.Categories.AddRange(defaultCats);
        await db.SaveChangesAsync();

        return Ok(new { message = "Registration successful." });
    }


    // POST: api/auth/login

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (user == null || !passwordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = jwt.GenerateToken(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        });
    }
}
