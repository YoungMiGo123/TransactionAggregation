using Bogus;
using Marten;
using TransactionAggregation.API.Models;
using TransactionAggregation.API.Services;

namespace TransactionAggregation.API.BackgroundServices;

public class DataSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DataSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var autoSeed = _configuration.GetValue<bool>("DataSeeding:AutoSeed");
        
        if (!autoSeed)
        {
            _logger.LogInformation("Auto-seeding is disabled");
            return;
        }

        _logger.LogInformation("Starting data seeding...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            var categorizationService = scope.ServiceProvider.GetRequiredService<ICategorizationService>();

            await using var session = documentStore.LightweightSession();

            // Check if data already exists
            var existingCount = await session.Query<Transaction>().CountAsync(stoppingToken);
            if (existingCount > 0)
            {
                _logger.LogInformation("Database already contains {Count} transactions. Skipping seed", existingCount);
                return;
            }

            // Generate transactions from multiple sources
            var allTransactions = new List<Transaction>();

            // Generate from Bank System
            allTransactions.AddRange(GenerateBankTransactions(categorizationService));

            // Generate from Credit Card System
            allTransactions.AddRange(GenerateCreditCardTransactions(categorizationService));

            // Generate from Payment Processor
            allTransactions.AddRange(GeneratePaymentProcessorTransactions(categorizationService));

            _logger.LogInformation("Generated {Count} transactions. Storing in database...", allTransactions.Count);

            // Store all transactions
            session.Store(allTransactions.ToArray());
            await session.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Successfully seeded {Count} transactions", allTransactions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding data");
        }
    }

    private List<Transaction> GenerateBankTransactions(ICategorizationService categorizationService)
    {
        var customerIds = new[] { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                  "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };
        
        var groceryStores = new[] { "Walmart Supercenter", "Target", "Kroger", "Safeway", "Whole Foods", "Trader Joe's" };
        var utilities = new[] { "Electric Company", "Water Utility", "Gas Company", "Internet Provider", "Phone Company" };
        var gasStations = new[] { "Shell Gas Station", "Exxon", "Chevron", "BP Gas", "Texaco" };

        var faker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow))
            .RuleFor(t => t.Source, f => "Bank System")
            .RuleFor(t => t.Currency, f => "USD")
            .RuleFor(t => t.Category, f => string.Empty)
            .RuleFor(t => t.Type, (f, t) => t.Amount > 0 ? "Credit" : "Debit")
            .RuleFor(t => t.Amount, f => f.Random.Bool(0.2f) ? 
                f.Random.Decimal(1000, 5000) : 
                -f.Random.Decimal(10, 500))
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
            })
            .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow)
            .RuleFor(t => t.IsDeleted, f => false);

        var transactions = faker.Generate(500);

        // Categorize all transactions
        foreach (var transaction in transactions)
        {
            transaction.Category = categorizationService.CategorizeTransaction(transaction);
        }

        return transactions;
    }

    private List<Transaction> GenerateCreditCardTransactions(ICategorizationService categorizationService)
    {
        var customerIds = new[] { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                  "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };
        
        var onlineStores = new[] { "Amazon.com", "eBay", "Etsy", "Best Buy Online", "Walmart.com", "Target.com" };
        var restaurants = new[] { "Italian Bistro", "Sushi Restaurant", "Steakhouse", "Pizza Place", "Mexican Grill", "Chinese Restaurant" };
        var transportServices = new[] { "Uber Ride", "Lyft", "Taxi Service", "Car Rental" };
        var airlines = new[] { "Delta Airlines", "United Airlines", "American Airlines", "Southwest Airlines" };
        var pharmacies = new[] { "CVS Pharmacy", "Walgreens", "Rite Aid", "Local Pharmacy" };
        var entertainmentServices = new[] { "Netflix", "Spotify Premium", "Hulu", "Disney+", "HBO Max", "YouTube Premium" };

        var faker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow))
            .RuleFor(t => t.Source, f => "Credit Card System")
            .RuleFor(t => t.Currency, f => "USD")
            .RuleFor(t => t.Category, f => string.Empty)
            .RuleFor(t => t.Amount, f => -f.Random.Decimal(5, 800))
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
            })
            .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow)
            .RuleFor(t => t.IsDeleted, f => false);

        var transactions = faker.Generate(400);

        // Categorize all transactions
        foreach (var transaction in transactions)
        {
            transaction.Category = categorizationService.CategorizeTransaction(transaction);
        }

        return transactions;
    }

    private List<Transaction> GeneratePaymentProcessorTransactions(ICategorizationService categorizationService)
    {
        var customerIds = new[] { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005", 
                                  "CUST-006", "CUST-007", "CUST-008", "CUST-009", "CUST-010" };
        
        var educationPlatforms = new[] { "Udemy", "Coursera", "LinkedIn Learning", "Skillshare", "Pluralsight" };
        var hotels = new[] { "Marriott Hotel", "Hilton", "Holiday Inn", "Best Western", "Hyatt", "Airbnb Rental" };
        var clothingStores = new[] { "H&M", "Zara", "Gap", "Old Navy", "Nordstrom", "Macy's" };
        var entertainmentVenues = new[] { "Theater Tickets", "Concert Tickets", "Movie Theater", "Sports Event", "Museum" };
        var subscriptionServices = new[] { "Adobe Creative Cloud", "Microsoft 365", "Dropbox Premium", "iCloud Storage" };

        var faker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow))
            .RuleFor(t => t.Source, f => "Payment Processor")
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
            })
            .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow)
            .RuleFor(t => t.IsDeleted, f => false);

        var transactions = faker.Generate(350);

        // Categorize all transactions
        foreach (var transaction in transactions)
        {
            transaction.Category = categorizationService.CategorizeTransaction(transaction);
        }

        return transactions;
    }
}
