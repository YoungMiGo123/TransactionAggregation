using TransactionAggregation.API.Models;

namespace TransactionAggregation.API.DataSources;

public interface ITransactionDataSource
{
    string SourceName { get; }
    Task<IEnumerable<Transaction>> GetTransactionsAsync();
    Task<IEnumerable<Transaction>> GetTransactionsByCustomerAsync(string customerId);
}
