using Bogus;
using TransactionAggregation.API.Application.Core.Entities;

namespace TransactionAggregation.API.Application.Features.Transactions.DataSources;

public class PaymentProcessorDataSource : ITransactionDataSource
{
    public string SourceName => "Payment Processor";
    private readonly List<Transaction> _transactions;
    private static readonly string[] CustomerIds = { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                                      "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };

    public PaymentProcessorDataSource()
    {
        _transactions = GenerateTransactions();
    }

    private List<Transaction> GenerateTransactions()
    {
        var faker = new Faker();

        var educationPlatforms = new[] { "Udemy", "Coursera", "LinkedIn Learning", "Skillshare", "Pluralsight" };
        var hotels = new[] { "Marriott Hotel", "Hilton", "Holiday Inn", "Best Western", "Hyatt", "Airbnb Rental" };
        var clothingStores = new[] { "H&M", "Zara", "Gap", "Old Navy", "Nordstrom", "Macy's" };
        var entertainmentVenues = new[] { "Theater Tickets", "Concert Tickets", "Movie Theater", "Sports Event", "Museum" };
        var subscriptionServices = new[] { "Adobe Creative Cloud", "Microsoft 365", "Dropbox Premium", "iCloud Storage" };

        var transactionFaker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(CustomerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.Now.AddDays(-90), DateTime.Now))
            .RuleFor(t => t.Source, f => SourceName)
            .RuleFor(t => t.Currency, f => "USD")
            .RuleFor(t => t.Category, f => string.Empty)
            .RuleFor(t => t.Amount, f => -f.Random.Decimal(15, 600))
            .RuleFor(t => t.Type, f => "Debit")
            .RuleFor(t => t.Description, (f, t) =>
            {
                var category = f.Random.Number(1, 5);
                return category switch
                {
                    1 => $"Online Course - {f.PickRandom(educationPlatforms)}",
                    2 => $"Hotel Reservation - {f.PickRandom(hotels)}",
                    3 => $"Clothing Store - {f.PickRandom(clothingStores)}",
                    4 => f.PickRandom(entertainmentVenues),
                    _ => f.PickRandom(subscriptionServices)
                };
            });

        return transactionFaker.Generate(350);
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
