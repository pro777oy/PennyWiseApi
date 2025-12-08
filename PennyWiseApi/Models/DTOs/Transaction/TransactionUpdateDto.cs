using PennyWiseApi.Models.Entities;

namespace PennyWiseApi.Models.DTOs.Transaction;

public class TransactionUpdateDto
{
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }

    public DateTime Date { get; set; }
    public string? Description { get; set; }
}