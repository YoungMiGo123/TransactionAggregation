using Bogus;
using TransactionAggregation.API.Application.Core.Entities;

namespace TransactionAggregation.API.Application.Features.Transactions.DataSources;

public class BankDataSource : ITransactionDataSource
{
    public string SourceName => "Bank System";
    private readonly List<Transaction> _transactions;
    private static readonly string[] CustomerIds = { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                                      "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };

    public BankDataSource()
    {
        _transactions = GenerateTransactions();
    }

    private List<Transaction> GenerateTransactions()
    {
        var faker = new Faker();

        var groceryStores = new[] { "Walmart Supercenter", "Target", "Kroger", "Safeway", "Whole Foods", "Trader Joe's" };
        var utilities = new[] { "Electric Company", "Water Utility", "Gas Company", "Internet Provider", "Phone Company" };
        var gasStations = new[] { "Shell Gas Station", "Exxon", "Chevron", "BP Gas", "Texaco" };

        var transactionFaker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(CustomerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.Now.AddDays(-90), DateTime.Now))
            .RuleFor(t => t.Source, f => SourceName)
            .RuleFor(t => t.Currency, f => "ZAR")
            .RuleFor(t => t.Category, f => string.Empty)
            .RuleFor(t => t.Type, (f, t) => t.Amount > 0 ? "Credit" : "Debit")
            .RuleFor(t => t.Amount, f => f.Random.Bool(0.2f) ? 
                f.Random.Decimal(1000, 5000) : // 20% chance of salary/credit
                -f.Random.Decimal(10, 500))     // 80% chance of expense
            .RuleFor(t => t.Description, (f, t) =>
            {
                if (t.Amount > 0)
                    return f.PickRandom(new[] { "Monthly Salary Deposit", "Bonus Payment", "Refund", "Interest Payment" });
                
                var category = f.Random.Number(1, 4);
                return category switch
                {
                    1 => $"{f.PickRandom(groceryStores)} Purchase",
                    2 => $"{f.PickRandom(utilities)} Payment",
                    3 => $"{f.PickRandom(gasStations)}",
                    _ => f.Commerce.ProductName()
                };
            });

        return transactionFaker.Generate(500);
    }

    public Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        return Task.FromResult(_transactions.AsEnumerable());
    }

    public Task<IEnumerable<Transaction>> GetTransactionsByCustomerAsync(string customerId)
    {
        return Task.FromResult(_transactions.Where(t => t.CustomerId == customerId).AsEnumerable());
    }
}
