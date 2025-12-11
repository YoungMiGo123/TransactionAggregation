using TransactionAggregation.API.Application.Core.Entities;

namespace TransactionAggregation.API.Application.Features.Transactions.DataSources;

public interface ITransactionDataSource
{
    string SourceName { get; }
    Task<IEnumerable<Transaction>> GetTransactionsAsync();
    Task<IEnumerable<Transaction>> GetTransactionsByCustomerAsync(string customerId);
}
