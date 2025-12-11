using Microsoft.AspNetCore.Mvc;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Application.Features.Shared;
using TransactionAggregation.API.Application.Features.Transactions.Handlers;
using Wolverine;

namespace TransactionAggregation.API.Application.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMessageBus messageBus) : BaseController
{
    /// <summary>
    /// Find transactions by multiple search criteria (at least one required)
    /// </summary>
    /// <remarks>
    /// Search transactions using one or more of the following criteria:
    /// - Id: Exact transaction ID match
    /// - CustomerId: Exact customer ID match
    /// - CustomerName: Partial name match (contains)
    /// - MinAmount/MaxAmount: Amount range filter
    /// - StartDate/EndDate: Date range filter
    /// - Description: Partial description match (contains)
    /// - Category: Exact category match
    /// - Source: Exact source match
    /// - Currency: Exact currency match
    /// - Type: Exact type match (Debit/Credit)
    /// 
    /// At least one search criterion must be provided.
    /// </remarks>
    [HttpGet("find")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FindTransactions([FromQuery] FindTransactionsQuery query)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
    }

    /// <summary>
    /// Get all transactions from all data sources
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTransactions([FromQuery] GetAllTransactionsQuery query)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
    }

    /// <summary>
    /// Get transactions for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByCustomer(string customerId, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionsByCustomerQuery
        {
            PageNo = pageNo,
            PageSize = pageSize,
            CustomerId = customerId
        };
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
    }

    /// <summary>
    /// Get transactions by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByCategory(string category, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionsByCategoryQuery
        {
            PageNo = pageNo,
            PageSize = pageSize,
            Category = category
        };
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
    }

    /// <summary>
    /// Get transactions within a date range
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsByDateRange(
       [FromQuery] GetTransactionsByDateRangeQuery query)
    {
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
    }

    /// <summary>
    /// Get transactions from a specific source
    /// </summary>
    [HttpGet("source/{source}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionsBySource(string source, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionsBySourceQuery
        {
            PageNo = pageNo,
            PageSize = pageSize,
            Source = source
        };
        
        var response = await messageBus.InvokeAsync<ApiResponse<TransactionsResponse>>(query);
        
        if (!response.Successful)
        {
            return FailedResponse(response);
        }
        
        return SuccessResponse(response);
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
        
        return SuccessResponse(response);
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
        
        return SuccessResponse(response);
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
        
        return SuccessResponse(response);
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
        
        return SuccessResponse(response);
    }
}
