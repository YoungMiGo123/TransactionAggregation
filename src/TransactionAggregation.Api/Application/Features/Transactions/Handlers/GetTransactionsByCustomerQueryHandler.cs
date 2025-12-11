using Marten;
using Marten.Pagination;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public class GetTransactionsByCustomerQuery : PaginationRequest
{
    public string CustomerId { get; set; } = string.Empty;
}


public class GetTransactionsByCustomerQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByCustomerQuery query, IDocumentSession session)
    {
        try
        {
            var totalCount = await session.Query<Transaction>()
                .Where(x => x.CustomerId == query.CustomerId && !x.IsDeleted)
                .CountAsync();

            var transactions = await session.Query<Transaction>()
                .Where(x => x.CustomerId == query.CustomerId && !x.IsDeleted)
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

            return ApiResponse<TransactionsResponse>.Success(response, $"Transactions for customer {query.CustomerId} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions for customer: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}