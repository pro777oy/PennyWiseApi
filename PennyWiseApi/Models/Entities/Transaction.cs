using System.ComponentModel.DataAnnotations.Schema;

namespace PennyWiseApi.Models.Entities;

public enum TransactionType
{
    Expense = 1,
    Income = 2
}

public class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }

    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime Date { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Category? Category { get; set; }
}