using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Application.Features.Transactions.DataSources;

namespace TransactionAggregation.API.Services;

public interface ITransactionAggregationService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<IEnumerable<Transaction>> GetTransactionsByCustomerAsync(string customerId);
    Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string category);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetTransactionsBySourceAsync(string source);
    Task<TransactionSummary> GetTransactionSummaryByCustomerAsync(string customerId);
    Task<Dictionary<string, decimal>> GetCategorySummaryAsync();
    Task<Dictionary<string, int>> GetTransactionCountBySourceAsync();
}

public class TransactionAggregationService : ITransactionAggregationService
{
    private readonly IEnumerable<ITransactionDataSource> _dataSources;
    private readonly ICategorizationService _categorizationService;

    public TransactionAggregationService(
        IEnumerable<ITransactionDataSource> dataSources,
        ICategorizationService categorizationService)
    {
        _dataSources = dataSources;
        _categorizationService = categorizationService;
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        var allTransactions = new List<Transaction>();

        foreach (var dataSource in _dataSources)
        {
            var transactions = await dataSource.GetTransactionsAsync();
            allTransactions.AddRange(transactions);
        }

        // Normalize and categorize
        return NormalizeAndCategorize(allTransactions);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCustomerAsync(string customerId)
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions.Where(t => t.CustomerId == customerId);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string category)
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsBySourceAsync(string source)
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions.Where(t => t.Source.Equals(source, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<TransactionSummary> GetTransactionSummaryByCustomerAsync(string customerId)
    {
        var transactions = (await GetTransactionsByCustomerAsync(customerId)).ToList();

        if (!transactions.Any())
        {
            return new TransactionSummary { CustomerId = customerId };
        }

        var summary = new TransactionSummary
        {
            CustomerId = customerId,
            CustomerName = transactions.First().CustomerName,
            TotalAmount = transactions.Sum(t => t.Amount),
            TransactionCount = transactions.Count,
            AverageTransactionAmount = transactions.Average(t => t.Amount),
            FirstTransactionDate = transactions.Min(t => t.TransactionDate),
            LastTransactionDate = transactions.Max(t => t.TransactionDate),
            Sources = transactions.Select(t => t.Source).Distinct().ToList()
        };

        // Category totals
        var categoryGroups = transactions.GroupBy(t => t.Category);
        foreach (var group in categoryGroups)
        {
            summary.CategoryTotals[group.Key] = group.Sum(t => t.Amount);
            summary.CategoryCounts[group.Key] = group.Count();
        }

        return summary;
    }

    public async Task<Dictionary<string, decimal>> GetCategorySummaryAsync()
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public async Task<Dictionary<string, int>> GetTransactionCountBySourceAsync()
    {
        var allTransactions = await GetAllTransactionsAsync();
        return allTransactions
            .GroupBy(t => t.Source)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private IEnumerable<Transaction> NormalizeAndCategorize(IEnumerable<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (transaction.Currency != "ZAR")
            {
                transaction.Currency = "ZAR";
            }

            if (string.IsNullOrEmpty(transaction.Category))
            {
                transaction.Category = _categorizationService.CategorizeTransaction(transaction);
            }

            if (transaction.TransactionDate.Kind != DateTimeKind.Utc)
            {
                transaction.TransactionDate = transaction.TransactionDate.ToUniversalTime();
            }
        }

        return transactions;
    }
}
