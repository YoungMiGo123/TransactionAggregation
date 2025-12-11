using TransactionAggregation.API.Application.Core.Entities;

namespace TransactionAggregation.API.Application.Core.Models;

public class TransactionsResponse
{
    public PaginationResponse<Transaction> Transactions { get; set; } 
}

public class CategoriesResponse
{
    public IEnumerable<string> Categories { get; set; } = new List<string>();
}

public class CategorySummaryResponse
{
    public Dictionary<string, CategoryBreakdown> CategoryBreakdowns { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalTransactions { get; set; }
}

public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageAmount { get; set; }
}

public class SourceSummaryResponse
{
    public Dictionary<string, SourceBreakdown> SourceBreakdowns { get; set; } = new();
    public int TotalTransactions { get; set; }
}

public class SourceBreakdown
{
    public string SourceName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}
