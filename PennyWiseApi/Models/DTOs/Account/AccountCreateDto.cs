namespace PennyWiseApi.Models.DTOs.Account;

public class AccountCreateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "BDT";
}