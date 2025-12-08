using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennyWiseApi.Data;
using PennyWiseApi.Models.Entities;
using System.Security.Claims;
using PennyWiseApi.Models.DTOs.Category;

namespace PennyWiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ApplicationDbContext db) : ControllerBase
{
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                  ?? User.FindFirstValue("sub");

        return Guid.Parse(sub!);
    }

    // GET: api/categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var userId = GetUserId();

        var list = await db.Categories
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Select(c => new CategoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .ToListAsync();

        return Ok(list);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var userId = GetUserId();

        var entity = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow,
            IsDefault = false
        };

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new { id = entity.Id });
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return NotFound();

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }
}
