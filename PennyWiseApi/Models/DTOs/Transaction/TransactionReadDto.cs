using PennyWiseApi.Models.Entities;

namespace PennyWiseApi.Models.DTOs.Transaction;

public class TransactionReadDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }

    public string AccountName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
}