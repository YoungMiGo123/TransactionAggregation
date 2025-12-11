using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Services;

namespace TransactionAggregation.API.Application.Services.Workers;

public class TransactionCategorizationService(
    IServiceProvider serviceProvider,
    ILogger<TransactionCategorizationService> logger)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit before starting to ensure app is fully initialized
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        logger.LogInformation("Transaction Categorization Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CategorizeUncategorizedTransactionsAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Transaction Categorization Service stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Transaction Categorization Service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CategorizeUncategorizedTransactionsAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
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

        logger.LogInformation("Found {Count} uncategorized transactions to process", uncategorizedTransactions.Count);

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
            logger.LogInformation("Categorized {Count} transactions", categorizedCount);
        }
    }
}
