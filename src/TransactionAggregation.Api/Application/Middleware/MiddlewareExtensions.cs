namespace TransactionAggregation.API.Application.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Adds global exception handler middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

