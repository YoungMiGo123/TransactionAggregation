using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Services;

public interface ICategorizationService
{
    string CategorizeTransaction(Transaction transaction);
}

public class CategorizationService : ICategorizationService
{
    private readonly Dictionary<string, List<string>> _categoryKeywords = new()
    {
        { TransactionCategory.Groceries, new List<string> { "walmart", "target", "grocery", "supermarket", "safeway", "kroger", "whole foods" } },
        { TransactionCategory.Entertainment, new List<string> { "netflix", "spotify", "hulu", "disney", "hbo", "theater", "cinema", "movie" } },
        { TransactionCategory.Utilities, new List<string> { "electric", "water", "gas", "internet", "phone", "utility", "bill" } },
        { TransactionCategory.Transportation, new List<string> { "uber", "lyft", "gas station", "shell", "exxon", "chevron", "parking", "transit" } },
        { TransactionCategory.Healthcare, new List<string> { "pharmacy", "cvs", "walgreens", "hospital", "clinic", "doctor", "medical", "health" } },
        { TransactionCategory.Shopping, new List<string> { "amazon", "ebay", "clothing", "h&m", "zara", "store", "mall" } },
        { TransactionCategory.Dining, new List<string> { "restaurant", "cafe", "bistro", "diner", "pizza", "burger", "starbucks", "coffee" } },
        { TransactionCategory.Travel, new List<string> { "flight", "hotel", "airline", "marriott", "hilton", "booking", "airbnb", "travel" } },
        { TransactionCategory.Education, new List<string> { "course", "udemy", "coursera", "school", "university", "tuition", "education", "learning" } }
    };

    public string CategorizeTransaction(Transaction transaction)
    {
        if (!string.IsNullOrEmpty(transaction.Category))
        {
            return transaction.Category;
        }

        var description = transaction.Description.ToLower();

        foreach (var category in _categoryKeywords)
        {
            if (category.Value.Any(keyword => description.Contains(keyword)))
            {
                return category.Key;
            }
        }

        return TransactionCategory.Other;
    }
}
