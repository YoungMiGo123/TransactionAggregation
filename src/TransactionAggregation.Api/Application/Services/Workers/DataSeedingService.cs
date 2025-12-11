using Bogus;
using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Services;

namespace TransactionAggregation.API.Application.Services.Workers;

public class DataSeedingService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<DataSeedingService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit before starting to ensure app is fully initialized
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        
        var autoSeed = configuration.GetValue<bool>("DataSeeding:AutoSeed");
        
        if (!autoSeed)
        {
            logger.LogInformation("Auto-seeding is disabled");
            return;
        }

        logger.LogInformation("Starting data seeding...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            var categorizer = scope.ServiceProvider.GetRequiredService<IRuleBasedCategorizer>();

            await using var session = documentStore.LightweightSession();

            // Seed customers first
            await SeedCustomersAsync(session, stoppingToken);

            // Seed categories and rules
            await SeedCategoriesAndRulesAsync(session, stoppingToken);

            // Check if transactions already exist
            var existingCount = await session.Query<Transaction>().CountAsync(stoppingToken);
            if (existingCount > 0)
            {
                logger.LogInformation("Database already contains {Count} transactions. Skipping transaction seed", existingCount);
                return;
            }

            // Generate transactions from multiple sources
            var allTransactions = new List<Transaction>();

            // Generate from Bank System
            allTransactions.AddRange(await GenerateBankTransactionsAsync(session, categorizer, stoppingToken));

            // Generate from Credit Card System
            allTransactions.AddRange(await GenerateCreditCardTransactionsAsync(session, categorizer, stoppingToken));

            // Generate from Payment Processor
            allTransactions.AddRange(await GeneratePaymentProcessorTransactionsAsync(session, categorizer, stoppingToken));

            logger.LogInformation("Generated {Count} transactions. Storing in database...", allTransactions.Count);

            // Store all transactions
            session.Store(allTransactions.ToArray());
            await session.SaveChangesAsync(stoppingToken);

            logger.LogInformation("Successfully seeded {Count} transactions", allTransactions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding data");
        }
    }

    private async Task SeedCustomersAsync(IDocumentSession session, CancellationToken stoppingToken)
    {
        var existingCustomers = await session.Query<Customer>().CountAsync(stoppingToken);
        if (existingCustomers > 0)
        {
            logger.LogInformation("Customers already seeded. Skipping.");
            return;
        }

        var customers = new List<Customer>
        {
            new Customer { CustomerId = "CUST-001", Name = "John Doe", Email = "john.doe@example.com" },
            new Customer { CustomerId = "CUST-002", Name = "Jane Smith", Email = "jane.smith@example.com" },
            new Customer { CustomerId = "CUST-003", Name = "Bob Johnson", Email = "bob.johnson@example.com" },
            new Customer { CustomerId = "CUST-004", Name = "Alice Williams", Email = "alice.williams@example.com" },
            new Customer { CustomerId = "CUST-005", Name = "Charlie Brown", Email = "charlie.brown@example.com" },
            new Customer { CustomerId = "CUST-006", Name = "Diana Prince", Email = "diana.prince@example.com" },
            new Customer { CustomerId = "CUST-007", Name = "Edward Norton", Email = "edward.norton@example.com" },
            new Customer { CustomerId = "CUST-008", Name = "Fiona Green", Email = "fiona.green@example.com" },
            new Customer { CustomerId = "CUST-009", Name = "George Miller", Email = "george.miller@example.com" },
            new Customer { CustomerId = "CUST-010", Name = "Hannah Davis", Email = "hannah.davis@example.com" }
        };

        session.Store(customers.ToArray());
        await session.SaveChangesAsync(stoppingToken);
        logger.LogInformation("Seeded {Count} customers", customers.Count);
    }

    private async Task SeedCategoriesAndRulesAsync(IDocumentSession session, CancellationToken stoppingToken)
    {
        var existingCategories = await session.Query<Category>().CountAsync(stoppingToken);
        if (existingCategories > 0)
        {
            logger.LogInformation("Categories already seeded. Skipping.");
            return;
        }

        var categories = new List<Category>
        {
            new Category
            {
                Name = TransactionCategory.Groceries,
                Description = "Grocery stores and supermarkets",
                Keywords = new List<string> { "walmart", "target", "grocery", "supermarket", "safeway", "kroger", "whole foods" }
            },
            new Category
            {
                Name = TransactionCategory.Entertainment,
                Description = "Entertainment and streaming services",
                Keywords = new List<string> { "netflix", "spotify", "hulu", "disney", "hbo", "theater", "cinema", "movie" }
            },
            new Category
            {
                Name = TransactionCategory.Utilities,
                Description = "Utility bills and services",
                Keywords = new List<string> { "electric", "water", "gas", "internet", "phone", "utility", "bill" }
            },
            new Category
            {
                Name = TransactionCategory.Transportation,
                Description = "Transportation and fuel",
                Keywords = new List<string> { "uber", "lyft", "gas station", "shell", "exxon", "chevron", "parking", "transit" }
            },
            new Category
            {
                Name = TransactionCategory.Healthcare,
                Description = "Healthcare and medical services",
                Keywords = new List<string> { "pharmacy", "cvs", "walgreens", "hospital", "clinic", "doctor", "medical", "health" }
            },
            new Category
            {
                Name = TransactionCategory.Shopping,
                Description = "Shopping and retail",
                Keywords = new List<string> { "amazon", "ebay", "clothing", "h&m", "zara", "store", "mall" }
            },
            new Category
            {
                Name = TransactionCategory.Dining,
                Description = "Restaurants and food services",
                Keywords = new List<string> { "restaurant", "cafe", "bistro", "diner", "pizza", "burger", "starbucks", "coffee" }
            },
            new Category
            {
                Name = TransactionCategory.Travel,
                Description = "Travel and accommodation",
                Keywords = new List<string> { "flight", "hotel", "airline", "marriott", "hilton", "booking", "airbnb", "travel" }
            },
            new Category
            {
                Name = TransactionCategory.Education,
                Description = "Education and learning",
                Keywords = new List<string> { "course", "udemy", "coursera", "school", "university", "tuition", "education", "learning" }
            },
            new Category
            {
                Name = TransactionCategory.Other,
                Description = "Other uncategorized transactions",
                Keywords = new List<string>()
            }
        };

        session.Store(categories.ToArray());
        await session.SaveChangesAsync(stoppingToken);
        logger.LogInformation("Seeded {Count} categories", categories.Count);

        // Now create category rules
        var rules = new List<CategoryRule>();
        var priority = 100;

        foreach (var category in categories.Where(c => c.Keywords.Any()))
        {
            foreach (var keyword in category.Keywords)
            {
                rules.Add(new CategoryRule
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Keyword = keyword,
                    Priority = priority--
                });
            }
        }

        session.Store(rules.ToArray());
        await session.SaveChangesAsync(stoppingToken);
        logger.LogInformation("Seeded {Count} category rules", rules.Count);
    }

    private async Task<List<Transaction>> GenerateBankTransactionsAsync(IDocumentSession session, IRuleBasedCategorizer categorizer, CancellationToken stoppingToken)
    {
        var customers = await session.Query<Customer>().Where(c => !c.IsDeleted).ToListAsync(stoppingToken);
        var customerIds = customers.Select(c => c.CustomerId).ToArray();
        
        var groceryStores = new[] { "Walmart Supercenter", "Target", "Kroger", "Safeway", "Whole Foods", "Trader Joe's" };
        var utilities = new[] { "Electric Company", "Water Utility", "Gas Company", "Internet Provider", "Phone Company" };
        var gasStations = new[] { "Shell Gas Station", "Exxon", "Chevron", "BP Gas", "Texaco" };

        var faker = new Faker<Transaction>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(t => t.CustomerName, f => f.Name.FullName())
            .RuleFor(t => t.TransactionDate, f => f.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow))
            .RuleFor(t => t.Source, f => "Bank System")
            .RuleFor(t => t.Currency, f => "ZAR")
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

        // Categorize all transactions using the rule-based categorizer
        foreach (var transaction in transactions)
        {
            transaction.Category = await categorizer.CategorizeTransactionAsync(transaction);
        }

        return transactions;
    }

    private async Task<List<Transaction>> GenerateCreditCardTransactionsAsync(IDocumentSession session, IRuleBasedCategorizer categorizer, CancellationToken stoppingToken)
    {
        var customers = await session.Query<Customer>().Where(c => !c.IsDeleted).ToListAsync(stoppingToken);
        var customerIds = customers.Select(c => c.CustomerId).ToArray();
        
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
            .RuleFor(t => t.Currency, f => "ZAR")
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

        // Categorize all transactions using the rule-based categorizer
        foreach (var transaction in transactions)
        {
            transaction.Category = await categorizer.CategorizeTransactionAsync(transaction);
        }

        return transactions;
    }

    private async Task<List<Transaction>> GeneratePaymentProcessorTransactionsAsync(IDocumentSession session, IRuleBasedCategorizer categorizer, CancellationToken stoppingToken)
    {
        var customers = await session.Query<Customer>().Where(c => !c.IsDeleted).ToListAsync(stoppingToken);
        var customerIds = customers.Select(c => c.CustomerId).ToArray();
        
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
            .RuleFor(t => t.Currency, f => "ZAR")
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

        // Categorize all transactions using the rule-based categorizer
        foreach (var transaction in transactions)
        {
            transaction.Category = await categorizer.CategorizeTransactionAsync(transaction);
        }

        return transactions;
    }
}
