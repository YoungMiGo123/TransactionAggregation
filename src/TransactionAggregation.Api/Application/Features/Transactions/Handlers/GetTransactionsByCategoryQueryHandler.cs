using Marten;
using Marten.Pagination;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public class GetTransactionsByCategoryQuery : PaginationRequest
{
    public string Category { get; set; } = string.Empty;
}


public class GetTransactionsByCategoryQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByCategoryQuery query, IDocumentSession session)
    {
        try
        {
            var totalCount = await session.Query<Transaction>()
                .Where(x => x.Category == query.Category && !x.IsDeleted)
                .CountAsync();

            var transactions = await session.Query<Transaction>()
                .Where(x => x.Category == query.Category && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToPagedListAsync(query.PageNo, query.PageSize);

            var paginationResponse = new PaginationResponse<Transaction>(
                transactions,
                query.PageNo,
                query.PageSize,
                (int)totalCount
            );

            var response = new TransactionsResponse
            {
                Transactions = paginationResponse,
            };

            return ApiResponse<TransactionsResponse>.Success(response, $"Transactions in category {query.Category} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions by category: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}