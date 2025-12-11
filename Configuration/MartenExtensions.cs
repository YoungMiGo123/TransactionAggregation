using Marten;
using TransactionAggregation.API.Models;

namespace TransactionAggregation.API.Configuration;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Use default connection string for development
            connectionString = "Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres";
        }

        services.AddMarten(opts =>
        {
            opts.Connection(connectionString);

            // Configure schema auto-creation
            // opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.CreateOrUpdate;

            // Configure document types with indexes
            ConfigureDocumentIndexes(opts);

        }).UseLightweightSessions(); // Use lightweight sessions (recommended for most scenarios)

        return services;
    }

    private static void ConfigureDocumentIndexes(StoreOptions options)
    {
        options.Policies.AllDocumentsSoftDeleted();

        // Transaction indexes
        options.Schema.For<Transaction>()
            .Identity(x => x.Id)
            .Index(x => x.TransactionDate)
            .Index(x => x.CustomerId)
            .Index(x => x.Category)
            .Index(x => x.Source)
            .Index(x => x.CreatedAt)
            .Duplicate(x => x.CustomerName)
            .Duplicate(x => x.Amount)
            .Duplicate(x => x.Currency)
            .Duplicate(x => x.Type)
            .Duplicate(x => x.Description);
    }
}
