namespace TransactionAggregation.API.Application.Core.Models;

public class ApiResponse<T>
{
    public bool Successful { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public List<string>? Errors { get; set; } = new();
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Successful = true,
            Message = message,
            Data = data,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public static ApiResponse<T> Fail(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return new ApiResponse<T>
        {
            Successful = false,
            Message = message,
            StatusCode = statusCode,
            Errors = [message]
        };
    }

    public static ApiResponse<T> Fail(List<string> errors, string message = "Operation failed", int statusCode = StatusCodes.Status400BadRequest)
    {
        return new ApiResponse<T>
        {
            Successful = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }
}
