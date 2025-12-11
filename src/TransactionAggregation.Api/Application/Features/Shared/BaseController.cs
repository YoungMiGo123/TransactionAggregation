using Microsoft.AspNetCore.Mvc;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Features.Shared;

public class BaseController : ControllerBase
{
    public IActionResult FailedResponse<T>(ApiResponse<T> response)
    {
        return response.StatusCode switch
        {
            404 => NotFound(response.Message),
            400 => BadRequest(response.Message),
            401 => Unauthorized(response.Message),
            _ => StatusCode(500, response.Message)
        };
    }
}
