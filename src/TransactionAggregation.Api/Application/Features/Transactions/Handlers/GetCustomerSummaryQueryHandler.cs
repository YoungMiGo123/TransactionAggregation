using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public record GetCustomerSummaryQuery(string CustomerId);

public class GetCustomerSummaryQueryHandler
{
    public async Task<ApiResponse<TransactionSummary>> Handle(GetCustomerSummaryQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => x.CustomerId == query.CustomerId && !x.IsDeleted)
                .ToListAsync();

            if (!transactions.Any())
            {
                return ApiResponse<TransactionSummary>.Fail($"No transactions found for customer {query.CustomerId}", StatusCodes.Status404NotFound);
            }

            var summary = new TransactionSummary
            {
                CustomerId = query.CustomerId,
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

            return ApiResponse<TransactionSummary>.Success(summary, $"Customer summary for {query.CustomerId} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionSummary>.Fail($"Error retrieving customer summary: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}