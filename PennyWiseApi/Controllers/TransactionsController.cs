using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennyWiseApi.Data;
using PennyWiseApi.Models.Entities;
using System.Security.Claims;
using PennyWiseApi.Models.DTOs.Transaction;

namespace PennyWiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController(ApplicationDbContext db) : ControllerBase
{
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        return Guid.Parse(sub!);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = GetUserId();

        var query = db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.IsDeleted == false);

        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value.Date);

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1); 
            query = query.Where(t => t.Date < end);
        }

        var data = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new TransactionReadDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Description = t.Description,
                AccountName = t.Account.Name,
                CategoryName = t.Category != null ? t.Category.Name : null
            })
            .ToListAsync();

        return Ok(data);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var userId = GetUserId();

        var t = await db.Transactions
            .AsNoTracking()
            .Include(x => x.Account)
            .Include(x => x.Category)
            .Where(x => x.UserId == userId && x.Id == id && x.IsDeleted == false)
            .Select(x => new TransactionReadDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                CategoryId = x.CategoryId,
                Amount = x.Amount,
                Type = x.Type,
                Date = x.Date,
                Description = x.Description,
                AccountName = x.Account.Name,
                CategoryName = x.Category != null ? x.Category.Name : null
            })
            .FirstOrDefaultAsync();

        if (t == null)
            return NotFound();

        return Ok(t);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();

      
        var accountExists = await db.Accounts
            .AnyAsync(a => a.Id == dto.AccountId && a.UserId == userId);

        if (!accountExists)
            return BadRequest("Invalid account.");

      
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId.Value);

            if (!categoryExists)
                return BadRequest("Invalid category.");
        }

        // Validate Type (1 = Expense, 2 = Income)
        if (dto.Type != TransactionType.Expense 
            && dto.Type != TransactionType.Income)
        {
            return BadRequest("Invalid Type. Must be Expense or Income.");
        }


        var entity = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Type = dto.Type,
            Date = dto.Date,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Transactions.Add(entity);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = entity.Id }, new { entity.Id });
    }
    
    
    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferCreateDto dto)
    {
        var userId = GetUserId();

        if (dto.FromAccountId == dto.ToAccountId)
            return BadRequest("Source and destination accounts must be different.");

        // Validate both accounts belong to the same user
        var accounts = await db.Accounts
            .Where(a => (a.Id == dto.FromAccountId || a.Id == dto.ToAccountId) 
                        && a.UserId == userId)
            .ToListAsync();

        if (accounts.Count != 2)
            return BadRequest("Invalid accounts for transfer.");

        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            // 1️⃣ Transfer OUT (expense from source)
            var outTxn = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = dto.FromAccountId,
                Amount = dto.Amount,
                Type = TransactionType.Expense,
                Description = dto.Description,
                Date = dto.Date,
                CreatedAt = DateTime.UtcNow
            };

            // 2️⃣ Transfer IN (income into destination)
            var inTxn = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = dto.ToAccountId,
                Amount = dto.Amount,
                Type = TransactionType.Income,
                Description = dto.Description,
                Date = dto.Date,
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(outTxn);
            db.Transactions.Add(inTxn);

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                success = true,
                transferOutId = outTxn.Id,
                transferInId = inTxn.Id
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }



    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] TransactionUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();

        var entity = await db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.IsDeleted == false);

        if (entity == null)
            return NotFound();


        if (dto.CategoryId.HasValue)
        {
            var catExists = await db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId.Value);

            if (!catExists)
                return BadRequest("Invalid category.");
        }

        if (dto.Type != TransactionType.Expense 
            && dto.Type != TransactionType.Income)
        {
            return BadRequest("Invalid Type. Must be Expense or Income.");
        }
        

        entity.CategoryId = dto.CategoryId;
        entity.Amount = dto.Amount;
        entity.Type = dto.Type;
        entity.Date = dto.Date;
        entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var userId = GetUserId();

        var entity = await db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.IsDeleted == false);

        if (entity == null)
            return NotFound();

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }
}
