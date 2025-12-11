using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public record GetSourceSummaryQuery;


public class GetSourceSummaryQueryHandler
{
    public async Task<ApiResponse<SourceSummaryResponse>> Handle(GetSourceSummaryQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var sourceGroups = transactions.GroupBy(t => t.Source);
            var sourceBreakdowns = new Dictionary<string, SourceBreakdown>();
            var totalCount = transactions.Count;

            foreach (var group in sourceGroups)
            {
                var transactionsList = group.ToList();
                var count = transactionsList.Count;
                
                sourceBreakdowns[group.Key] = new SourceBreakdown
                {
                    SourceName = group.Key,
                    TransactionCount = count,
                    TotalAmount = transactionsList.Sum(t => t.Amount),
                    Percentage = totalCount > 0 ? (decimal)count / totalCount * 100 : 0
                };
            }

            var response = new SourceSummaryResponse
            {
                SourceBreakdowns = sourceBreakdowns,
                TotalTransactions = totalCount
            };

            return ApiResponse<SourceSummaryResponse>.Success(response, "Source summary retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<SourceSummaryResponse>.Fail($"Error retrieving source summary: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}