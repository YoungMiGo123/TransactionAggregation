namespace TransactionAggregation.API.Application.Core.Models;

public class TransactionSummary
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public Dictionary<string, decimal> CategoryTotals { get; set; } = new();
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public DateTime? FirstTransactionDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public List<string> Sources { get; set; } = new();
}
