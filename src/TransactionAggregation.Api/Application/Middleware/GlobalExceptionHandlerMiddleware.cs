using System.Text.Json;
using TransactionAggregation.API.Application.Core.Models;

namespace TransactionAggregation.API.Application.Middleware;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        
        // Log the exception with correlation ID
        logger.LogError(exception, 
            "An unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}", 
            correlationId,
            context.Request.Path,
            context.Request.Method);

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errorDetails) = GetErrorResponse(exception);
        response.StatusCode = statusCode;

        // Build error response
        var errorResponse = new ApiResponse<object>
        {
            Successful = false,
            StatusCode = statusCode,
            Message = message,
            Data = null,
            Errors = errorDetails,
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        // Add detailed error information in development environment
        if (environment.IsDevelopment())
        {
            errorResponse.Errors = errorResponse.Errors ?? new List<string>();
            errorResponse.Errors.Add($"Exception Type: {exception.GetType().Name}");
            errorResponse.Errors.Add($"Stack Trace: {exception.StackTrace}");
            
            if (exception.InnerException != null)
            {
                errorResponse.Errors.Add($"Inner Exception: {exception.InnerException.Message}");
            }
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteAsync(jsonResponse);
    }

    private (int statusCode, string message, List<string>? errors) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => 
                (StatusCodes.Status400BadRequest, 
                 "A required argument was not provided.", 
                 new List<string> { exception.Message }),
            
            ArgumentException => 
                (StatusCodes.Status400BadRequest, 
                 "Invalid argument provided.", 
                 new List<string> { exception.Message }),
            
            InvalidOperationException => 
                (StatusCodes.Status400BadRequest, 
                 "The operation is invalid in the current state.", 
                 new List<string> { exception.Message }),
            
            UnauthorizedAccessException => 
                (StatusCodes.Status401Unauthorized, 
                 "Unauthorized access.", 
                 new List<string> { exception.Message }),
            
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, 
                 "The requested resource was not found.", 
                 new List<string> { exception.Message }),
            
            TimeoutException => 
                (StatusCodes.Status408RequestTimeout, 
                 "The request timed out.", 
                 new List<string> { exception.Message }),
            
            NotImplementedException => 
                (StatusCodes.Status501NotImplemented, 
                 "This feature is not yet implemented.", 
                 new List<string> { exception.Message }),
            
            _ => 
                (StatusCodes.Status500InternalServerError, 
                 "An unexpected error occurred. Please try again later.", 
                 environment.IsDevelopment() 
                     ? new List<string> { exception.Message } 
                     : null)
        };
    }
}


