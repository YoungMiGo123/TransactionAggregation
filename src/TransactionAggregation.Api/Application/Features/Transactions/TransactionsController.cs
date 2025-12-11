using Microsoft.AspNetCore.Mvc;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Application.Features.Shared;
using TransactionAggregation.API.Queries;
using Wolverine;

namespace TransactionAggregation.API.Application.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMessageBus messageBus) : BaseController
{
    /// <summary>
    /// Get all transactions from all data sources
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTransactions()
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(new GetAllTransactionsQuery());
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get transactions for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByCustomer(string customerId)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(new GetTransactionsByCustomerQuery(customerId));
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get transactions by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByCategory(string category)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(new GetTransactionsByCategoryQuery(category));
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get transactions within a date range
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(new GetTransactionsByDateRangeQuery(startDate, endDate));
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get transactions from a specific source
    /// </summary>
    [HttpGet("source/{source}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsBySource(string source)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(new GetTransactionsBySourceQuery(source));
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get comprehensive transaction summary for a customer
    /// </summary>
    [HttpGet("summary/customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerSummary(string customerId)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionSummary>>(new GetCustomerSummaryQuery(customerId));
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get total amounts by category across all customers
    /// </summary>
    [HttpGet("summary/categories")]
    [ProducesResponseType(typeof(ApiResponse<CategorySummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategorySummary()
    {
        var response = await messageBus.InvokeAsync<ApiResponse<CategorySummaryResponse>>(new GetCategorySummaryQuery());
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get transaction count by data source
    /// </summary>
    [HttpGet("summary/sources")]
    [ProducesResponseType(typeof(ApiResponse<SourceSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSourceSummary()
    {
        var response = await messageBus.InvokeAsync<ApiResponse<SourceSummaryResponse>>(new GetSourceSummaryQuery());
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get available transaction categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<CategoriesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {
        var response = await messageBus.InvokeAsync<ApiResponse<CategoriesResponse>>(new GetCategoriesQuery());
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return StatusCode(response.StatusCode, response);
    }
}
