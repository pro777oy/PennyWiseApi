using PennyWiseApi.Models.Entities;

namespace PennyWiseApi.Models.DTOs.Category;

public class CategoryReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
}
