# Global Exception Handling and Correlation ID

## Overview
This application implements comprehensive error handling and request tracking through middleware components.

## Features

### 1. Correlation ID Middleware
- **Purpose**: Tracks requests across the system with a unique identifier
- **Header**: `X-Correlation-ID`
- **Behavior**: 
  - Accepts correlation ID from incoming request headers
  - Generates new GUID if not provided
  - Adds correlation ID to response headers
  - Includes in all log entries for the request
  - Available in `HttpContext.Items["CorrelationId"]`

**Usage Example:**
```bash
# Request with correlation ID
curl -H "X-Correlation-ID: my-custom-id-123" http://localhost:5196/api/transactions

# Response will include the same correlation ID in headers and response body
```

### 2. Global Exception Handler Middleware
- **Purpose**: Catches all unhandled exceptions and returns consistent error responses
- **Features**:
  - Logs all exceptions with correlation ID
  - Returns standardized error responses
  - Provides detailed error info in Development environment
  - Protects sensitive information in Production

**Exception Mappings:**
- `ArgumentNullException` / `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 400 Bad Request
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- `TimeoutException` → 408 Request Timeout
- `NotImplementedException` → 501 Not Implemented
- Other exceptions → 500 Internal Server Error

### 3. Enhanced API Response Model
All API responses now include:
- `correlationId`: Unique identifier for request tracking
- `timestamp`: UTC timestamp of the response
- `successful`: Boolean indicating success/failure
- `message`: Human-readable message
- `data`: Response payload (if successful)
- `errors`: List of error messages (if failed)
- `statusCode`: HTTP status code

**Example Success Response:**
```json
{
  "successful": true,
  "message": "Transactions retrieved successfully",
  "data": {
    "transactions": {
      "payload": [...],
      "page": 1,
      "pageSize": 20,
      "totalCount": 150,
      "totalPages": 8
    }
  },
  "statusCode": 200,
  "errors": [],
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "timestamp": "2025-12-11T12:30:00Z"
}
```

**Example Error Response:**
```json
{
  "successful": false,
  "message": "An unexpected error occurred. Please try again later.",
  "data": null,
  "statusCode": 500,
  "errors": [
    "Database connection failed"
  ],
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "timestamp": "2025-12-11T12:30:00Z"
}
```

## Middleware Order
The middleware is registered in the following order (important for proper functioning):

1. **CorrelationIdMiddleware** - First, to ensure all requests have an ID
2. **GlobalExceptionHandlerMiddleware** - Second, to catch all downstream exceptions
3. Swagger (Development only)
4. HTTPS Redirection
5. CORS
6. Authorization
7. Controllers

## Logging
All requests are logged with the following information:
- HTTP Method
- Request Path
- Status Code
- Correlation ID
- Exceptions (if any)

**Log Format:**
```
[Information] Request started: GET /api/transactions - CorrelationId: abc-123
[Information] Request completed: GET /api/transactions - Status: 200 - CorrelationId: abc-123
[Error] An unhandled exception occurred. CorrelationId: abc-123, Path: /api/transactions, Method: GET
```

## Usage in Controllers
Controllers inherit from `BaseController` which provides helper methods:

```csharp
// For successful responses (automatically adds correlation ID)
return SuccessResponse(response);

// For failed responses (automatically adds correlation ID)
return FailedResponse(response);
```

## Testing Exception Handling

To test the exception handling, you can:

1. **Trigger a validation error** (400):
   ```bash
   GET /api/transactions/find
   # Without any search parameters
   ```

2. **Test not found** (404):
   ```bash
   GET /api/transactions/customer/non-existent-id
   ```

3. **Test with custom correlation ID**:
   ```bash
   curl -H "X-Correlation-ID: test-123" http://localhost:5196/api/transactions
   ```

## Benefits

1. **Debugging**: Correlation IDs make it easy to trace requests through logs
2. **Monitoring**: Consistent error responses enable better monitoring and alerting
3. **User Experience**: Clear, structured error messages
4. **Security**: Sensitive information hidden in production
5. **Consistency**: All endpoints return the same response structure

