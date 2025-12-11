using Marten;
using Marten.Pagination;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public class GetTransactionsByDateRangeQuery : PaginationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}


public class GetTransactionsByDateRangeQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByDateRangeQuery query, IDocumentSession session)
    {
        try
        {
            var totalCount = await session.Query<Transaction>()
                .Where(x => x.TransactionDate >= query.StartDate && 
                            x.TransactionDate <= query.EndDate && 
                            !x.IsDeleted)
                .CountAsync();

            var transactions = await session.Query<Transaction>()
                .Where(x => x.TransactionDate >= query.StartDate && 
                            x.TransactionDate <= query.EndDate && 
                            !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToPagedListAsync(query.PageNo, query.PageSize);

            var paginationResponse = new PaginationResponse<Transaction>(
                transactions,
                query.PageNo,
                query.PageSize,
                totalCount
            );

            var response = new TransactionsResponse
            {
                Transactions = paginationResponse,
            };

            return ApiResponse<TransactionsResponse>.Success(response, "Transactions in date range retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions by date range: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}