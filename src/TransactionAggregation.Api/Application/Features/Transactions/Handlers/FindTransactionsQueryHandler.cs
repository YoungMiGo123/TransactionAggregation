using System.ComponentModel.DataAnnotations;
using Marten;
using Marten.Pagination;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public class FindTransactionsQuery : PaginationRequest
{
    public Guid? Id { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public string? Currency { get; set; }
    public string? Type { get; set; }
    /// <summary>
    /// Validates that at least one search criterion is provided
    /// </summary>
    public bool HasSearchCriteria()
    {
        return Id.HasValue ||
               !string.IsNullOrWhiteSpace(CustomerId) ||
               !string.IsNullOrWhiteSpace(CustomerName) ||
               MinAmount.HasValue ||
               MaxAmount.HasValue ||
               StartDate.HasValue ||
               EndDate.HasValue ||
               !string.IsNullOrWhiteSpace(Description) ||
               !string.IsNullOrWhiteSpace(Category) ||
               !string.IsNullOrWhiteSpace(Source) ||
               !string.IsNullOrWhiteSpace(Currency) ||
               !string.IsNullOrWhiteSpace(Type);
    }
}

public class FindTransactionsQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(FindTransactionsQuery query, IDocumentSession session)
    {
        try
        {
            // Validate at least one search criterion is provided
            if (!query.HasSearchCriteria())
            {
                return ApiResponse<TransactionsResponse>.Fail(
                    "At least one search criterion is required",
                    StatusCodes.Status400BadRequest);
            }

            // Build the query dynamically based on provided criteria
            var baseQuery = session.Query<Transaction>().Where(x => !x.IsDeleted);

            if (query.Id.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Id == query.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.CustomerId))
            {
                baseQuery = baseQuery.Where(x => x.CustomerId == query.CustomerId);
            }

            if (!string.IsNullOrWhiteSpace(query.CustomerName))
            {
                baseQuery = baseQuery.Where(x => x.CustomerName.Contains(query.CustomerName));
            }

            if (query.MinAmount.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Amount >= query.MinAmount.Value);
            }

            if (query.MaxAmount.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Amount <= query.MaxAmount.Value);
            }

            if (query.StartDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.TransactionDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.TransactionDate <= query.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Description))
            {
                baseQuery = baseQuery.Where(x => x.Description.Contains(query.Description));
            }

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                baseQuery = baseQuery.Where(x => x.Category == query.Category);
            }

            if (!string.IsNullOrWhiteSpace(query.Source))
            {
                baseQuery = baseQuery.Where(x => x.Source == query.Source);
            }

            if (!string.IsNullOrWhiteSpace(query.Currency))
            {
                baseQuery = baseQuery.Where(x => x.Currency == query.Currency);
            }

            if (!string.IsNullOrWhiteSpace(query.Type))
            {
                baseQuery = baseQuery.Where(x => x.Type == query.Type);
            }

            // Get total count
            var totalCount = await baseQuery.CountAsync();

            // Get paginated results
            var transactions = await baseQuery
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

            return ApiResponse<TransactionsResponse>.Success(
                response,
                $"Found {totalCount} transaction(s) matching search criteria");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail(
                $"Error searching transactions: {ex.Message}",
                StatusCodes.Status500InternalServerError);
        }
    }
}

