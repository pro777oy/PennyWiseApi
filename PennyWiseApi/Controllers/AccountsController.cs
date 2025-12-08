using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennyWiseApi.Data;
using PennyWiseApi.Models.Entities;
using System.Security.Claims;
using PennyWiseApi.Models.DTOs.Account;

namespace PennyWiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController(ApplicationDbContext db) : ControllerBase
{
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.Parse(sub!);
    }

    // GET ALL ACCOUNTS
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = GetUserId();

        var list = await db.Accounts
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .Select(a => new AccountReadDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                InitialBalance = a.InitialBalance,
                Currency = a.Currency,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    // CREATE ACCOUNT
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] AccountCreateDto dto)
    {
        var userId = GetUserId();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            InitialBalance = dto.InitialBalance,
            Currency = dto.Currency,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return Ok(new { id = account.Id });
    }

    // UPDATE ACCOUNT
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] AccountCreateDto dto)
    {
        var userId = GetUserId();

        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
            return NotFound();

        account.Name = dto.Name;
        account.Description = dto.Description;
        account.InitialBalance = dto.InitialBalance;
        account.Currency = dto.Currency;
        account.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE ACCOUNT (soft delete)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var userId = GetUserId();
        
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
            return NotFound();

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }
}
