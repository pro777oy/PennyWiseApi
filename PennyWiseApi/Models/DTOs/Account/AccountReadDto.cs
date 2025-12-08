namespace PennyWiseApi.Models.DTOs.Account;

public class AccountReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}