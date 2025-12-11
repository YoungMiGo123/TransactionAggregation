using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public record GetCategorySummaryQuery;


public class GetCategorySummaryQueryHandler
{
    public async Task<ApiResponse<CategorySummaryResponse>> Handle(GetCategorySummaryQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var categoryGroups = transactions.GroupBy(t => t.Category);
            var categoryBreakdowns = new Dictionary<string, CategoryBreakdown>();

            foreach (var group in categoryGroups)
            {
                var transactionsList = group.ToList();
                categoryBreakdowns[group.Key] = new CategoryBreakdown
                {
                    CategoryName = group.Key,
                    TotalAmount = transactionsList.Sum(t => t.Amount),
                    TransactionCount = transactionsList.Count,
                    AverageAmount = transactionsList.Average(t => t.Amount)
                };
            }

            var response = new CategorySummaryResponse
            {
                CategoryBreakdowns = categoryBreakdowns,
                TotalAmount = transactions.Sum(t => t.Amount),
                TotalTransactions = transactions.Count
            };

            return ApiResponse<CategorySummaryResponse>.Success(response, "Category summary retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CategorySummaryResponse>.Fail($"Error retrieving category summary: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}