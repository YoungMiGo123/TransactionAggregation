namespace TransactionAggregation.API.Application.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Add to response headers
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);
        
        // Add to HttpContext items for access throughout the request pipeline
        context.Items["CorrelationId"] = correlationId;
        
        // Add to logging scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            _logger.LogInformation("Request started: {Method} {Path} - CorrelationId: {CorrelationId}", 
                context.Request.Method, 
                context.Request.Path, 
                correlationId);
            
            await _next(context);
            
            _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - CorrelationId: {CorrelationId}", 
                context.Request.Method, 
                context.Request.Path, 
                context.Response.StatusCode,
                correlationId);
        }
    }

    private string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if correlation ID exists in request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }
}

