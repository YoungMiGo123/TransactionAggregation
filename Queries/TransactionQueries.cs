using TransactionAggregation.API.Models;

namespace TransactionAggregation.API.Queries;

// Query to get all transactions
public record GetAllTransactionsQuery;

// Query to get transactions by customer
public record GetTransactionsByCustomerQuery(string CustomerId);

// Query to get transactions by category
public record GetTransactionsByCategoryQuery(string Category);

// Query to get transactions by date range
public record GetTransactionsByDateRangeQuery(DateTime StartDate, DateTime EndDate);

// Query to get transactions by source
public record GetTransactionsBySourceQuery(string Source);

// Query to get customer summary
public record GetCustomerSummaryQuery(string CustomerId);

// Query to get category summary
public record GetCategorySummaryQuery;

// Query to get source summary
public record GetSourceSummaryQuery;
