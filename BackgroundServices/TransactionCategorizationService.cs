using Marten;
using TransactionAggregation.API.Models;
using TransactionAggregation.API.Services;

namespace TransactionAggregation.API.BackgroundServices;

public class TransactionCategorizationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionCategorizationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public TransactionCategorizationService(
        IServiceProvider serviceProvider,
        ILogger<TransactionCategorizationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Transaction Categorization Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CategorizeUncategorizedTransactionsAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Transaction Categorization Service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Transaction Categorization Service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CategorizeUncategorizedTransactionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var categorizer = scope.ServiceProvider.GetRequiredService<IRuleBasedCategorizer>();

        await using var session = documentStore.LightweightSession();

        // Find transactions that are not categorized or have "Other" category
        var uncategorizedTransactions = await session.Query<Transaction>()
            .Where(t => !t.IsDeleted && (t.Category == string.Empty || t.Category == TransactionCategory.Other || t.Category == null))
            .Take(100) // Process in batches
            .ToListAsync(stoppingToken);

        if (!uncategorizedTransactions.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} uncategorized transactions to process", uncategorizedTransactions.Count);

        var categorizedCount = 0;
        foreach (var transaction in uncategorizedTransactions)
        {
            var category = await categorizer.CategorizeTransactionAsync(transaction);
            if (category != transaction.Category)
            {
                transaction.Category = category;
                transaction.UpdatedAt = DateTime.UtcNow;
                session.Update(transaction);
                categorizedCount++;
            }
        }

        if (categorizedCount > 0)
        {
            await session.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Categorized {Count} transactions", categorizedCount);
        }
    }
}
