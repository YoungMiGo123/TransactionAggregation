using Marten;
using TransactionAggregation.API.Models;
using TransactionAggregation.API.Queries;

namespace TransactionAggregation.API.Queries;

public class GetAllTransactionsQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetAllTransactionsQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var response = new TransactionsResponse
            {
                Transactions = transactions,
                TotalCount = transactions.Count
            };

            return ApiResponse<TransactionsResponse>.Success(response, "Transactions retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}

public class GetTransactionsByCustomerQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByCustomerQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => x.CustomerId == query.CustomerId && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var response = new TransactionsResponse
            {
                Transactions = transactions,
                TotalCount = transactions.Count
            };

            return ApiResponse<TransactionsResponse>.Success(response, $"Transactions for customer {query.CustomerId} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions for customer: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}

public class GetTransactionsByCategoryQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByCategoryQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => x.Category == query.Category && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var response = new TransactionsResponse
            {
                Transactions = transactions,
                TotalCount = transactions.Count
            };

            return ApiResponse<TransactionsResponse>.Success(response, $"Transactions in category {query.Category} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions by category: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}

public class GetTransactionsByDateRangeQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsByDateRangeQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => x.TransactionDate >= query.StartDate && 
                           x.TransactionDate <= query.EndDate && 
                           !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var response = new TransactionsResponse
            {
                Transactions = transactions,
                TotalCount = transactions.Count
            };

            return ApiResponse<TransactionsResponse>.Success(response, "Transactions in date range retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions by date range: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}

public class GetTransactionsBySourceQueryHandler
{
    public async Task<ApiResponse<TransactionsResponse>> Handle(GetTransactionsBySourceQuery query, IDocumentSession session)
    {
        try
        {
            var transactions = await session.Query<Transaction>()
                .Where(x => x.Source == query.Source && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var response = new TransactionsResponse
            {
                Transactions = transactions,
                TotalCount = transactions.Count
            };

            return ApiResponse<TransactionsResponse>.Success(response, $"Transactions from source {query.Source} retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<TransactionsResponse>.Fail($"Error retrieving transactions by source: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}

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

public class GetCategoriesQueryHandler
{
    public async Task<ApiResponse<CategoriesResponse>> Handle(GetCategoriesQuery query, IDocumentSession session)
    {
        try
        {
            var categories = await session.Query<Category>()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categoryNames = categories.Select(c => c.Name).ToList();

            var response = new CategoriesResponse
            {
                Categories = categoryNames
            };

            return ApiResponse<CategoriesResponse>.Success(response, "Categories retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriesResponse>.Fail($"Error retrieving categories: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
