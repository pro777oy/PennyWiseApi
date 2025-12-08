using PennyWiseApi.Models.Entities;

namespace PennyWiseApi.Models.DTOs.Category;

public class CategoryCreateDto
{
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
}