namespace TransactionAggregation.API.Application.Core.Entities;

public class Transaction : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Currency { get; set; } = "ZAR";
    public string Type { get; set; } = string.Empty; // Debit or Credit
}
