using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Services;

public interface IRuleBasedCategorizer
{
    Task<string> CategorizeTransactionAsync(Transaction transaction);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<CategoryRule>> GetCategoryRulesAsync();
}

public class RuleBasedCategorizer : IRuleBasedCategorizer
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<RuleBasedCategorizer> _logger;

    public RuleBasedCategorizer(IDocumentStore documentStore, ILogger<RuleBasedCategorizer> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task<string> CategorizeTransactionAsync(Transaction transaction)
    {
        // If already categorized, return existing category
        if (!string.IsNullOrEmpty(transaction.Category))
        {
            return transaction.Category;
        }

        await using var session = _documentStore.LightweightSession();

        // Get all category rules ordered by priority
        var rules = await session.Query<CategoryRule>()
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        if (!rules.Any())
        {
            _logger.LogWarning("No categorization rules found in database");
            return TransactionCategory.Other;
        }

        var description = transaction.Description.ToLower();

        // Apply rules in priority order
        foreach (var rule in rules)
        {
            if (description.Contains(rule.Keyword.ToLower()))
            {
                return rule.CategoryName;
            }
        }

        return TransactionCategory.Other;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await using var session = _documentStore.LightweightSession();
        var categories = await session.Query<Category>()
            .Where(c => !c.IsDeleted)
            .ToListAsync();
        return categories.ToList();
    }

    public async Task<List<CategoryRule>> GetCategoryRulesAsync()
    {
        await using var session = _documentStore.LightweightSession();
        var rules = await session.Query<CategoryRule>()
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
        return rules.ToList();
    }
}
