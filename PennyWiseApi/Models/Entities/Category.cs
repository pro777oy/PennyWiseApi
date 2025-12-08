using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PennyWiseApi.Models.Entities;

public enum CategoryType
{
    Expense = 1,
    Income = 2
}

public class Category
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public CategoryType Type { get; set; }

    public bool IsDefault { get; set; }  

    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}