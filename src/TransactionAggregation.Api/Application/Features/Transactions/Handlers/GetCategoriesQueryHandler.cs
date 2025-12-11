using Marten;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Transactions.Handlers;

public record GetCategoriesQuery;

public class GetCategoriesQueryHandler
{
    public async Task<ApiResponse<CategoriesResponse>> Handle(GetCategoriesQuery query, IDocumentSession session)
    {
        try
        {
            var categories = await session.Query<Category>()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categoryNames = categories.Select(c => c.Name).ToList();

            var response = new CategoriesResponse
            {
                Categories = categoryNames
            };

            return ApiResponse<CategoriesResponse>.Success(response, "Categories retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriesResponse>.Fail($"Error retrieving categories: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
