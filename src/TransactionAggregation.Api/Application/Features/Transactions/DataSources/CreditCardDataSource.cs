using Bogus;
using TransactionAggregation.API.Application.Core.Entities;

namespace TransactionAggregation.API.Application.Features.Transactions.DataSources;

public class CreditCardDataSource : ITransactionDataSource
{
    public string SourceName => "Credit Card System";
    private readonly List<Transaction> _transactions;
    private static readonly string[] CustomerIds = { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                                      "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };

    public CreditCardDataSource()
    {
        _transactions = GenerateTransactions();
    }

    private List<Transaction> GenerateTransactions()
    {
        var faker = new Faker();

        var onlineStores = new[] { "Amazon.com", "eBay", "Etsy", "Best Buy Online", "Walmart.com", "Target.com" };
        var restaurants = new[] { "Italian Bistro", "Sushi Restaurant", "Steakhouse", "Pizza Place", "Mexican Grill", "Chinese Restaurant" };
        var transportServices = new[] { "Uber Ride", "Lyft", "Taxi Service", "Car Rental" };
        var airlines = new[] { "Delta Airlines", "United Airlines", "American Airlines", "Southwest Airlines" };
        var pharmacies = new[] { "CVS Pharmacy", "Walgreens", "Rite Aid", "Local Pharmacy" };
        var entertainmentServices = new[] { "Netflix", "Spotify Premium", "Hulu", "Disney+", "HBO Max", "YouTube Premium" };

        var transactionFaker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(CustomerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.Now.AddDays(-90), DateTime.Now))
            .RuleFor(t => t.Source, f => SourceName)
            .RuleFor(t => t.Currency, f => "USD")
            .RuleFor(t => t.Category, f => string.Empty)
            .RuleFor(t => t.Amount, f => -f.Random.Decimal(5, 800)) // Credit cards are typically expenses
            .RuleFor(t => t.Type, f => "Debit")
            .RuleFor(t => t.Description, (f, t) =>
            {
                var category = f.Random.Number(1, 6);
                return category switch
                {
                    1 => $"{f.PickRandom(onlineStores)} Purchase",
                    2 => $"Restaurant - {f.PickRandom(restaurants)}",
                    3 => f.PickRandom(transportServices),
                    4 => $"Flight Booking - {f.PickRandom(airlines)}",
                    5 => $"Pharmacy - {f.PickRandom(pharmacies)}",
                    _ => f.PickRandom(entertainmentServices)
                };
            });

        return transactionFaker.Generate(400);
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
