using Microsoft.AspNetCore.Mvc;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Shared;

public class BaseController : ControllerBase
{
    protected string GetCorrelationId()
    {
        return HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    }

    protected void AttachCorrelationId<T>(ApiResponse<T> response)
    {
        response.CorrelationId = GetCorrelationId();
        response.Timestamp = DateTime.UtcNow;
    }

    public IActionResult FailedResponse<T>(ApiResponse<T> response)
    {
        AttachCorrelationId(response);
        
        return response.StatusCode switch
        {
            404 => NotFound(response),
            400 => BadRequest(response),
            401 => Unauthorized(response),
            _ => StatusCode(500, response)
        };
    }

    protected IActionResult SuccessResponse<T>(ApiResponse<T> response)
    {
        AttachCorrelationId(response);
        return StatusCode(response.StatusCode, response);
    }
}
