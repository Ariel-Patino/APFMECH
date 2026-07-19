---
name: APFMech Logging & Middleware Skill
description: Enforces global exception handling, standardised RFC 7807 error responses, and structured JSON logging with Serilog, including correlation ID propagation for end‑to‑end traceability.
version: 1.0.0
---

# 07-logging-middleware.skill.md

## Purpose

This skill governs how the APFMech .NET 10 backend captures, processes, and logs exceptions, and how it emits structured, searchable log events. It establishes the non‑negotiable rules for:

- A custom global exception handling middleware that intercepts all unhandled exceptions and transforms them into RFC 7807 Problem Details responses.
- Structured logging using **Serilog** with semantic properties and JSON output sinks.
- Correlation ID propagation across all log entries to enable distributed tracing and debugging.

All generated middleware, logging statements, and error handling code must adhere to these standards to ensure operational visibility and a consistent, secure error surface for clients.

---

## Global Exception Middleware Rules

### 1. Mandatory Custom Middleware

- **Every unhandled exception** that escapes the application layers must be caught by a dedicated custom middleware registered at the top of the request pipeline (before any endpoint routing).
- The middleware must **never** expose raw exception messages or stack traces to the HTTP client in production. All exceptions are transformed into a **clean, RFC 7807 Problem Details** JSON response.
- The middleware must **log** the full exception (including stack trace and inner exceptions) using Serilog before constructing the response.

### 2. Exception to HTTP Status Code Mapping

The middleware must map specific exception types to the appropriate HTTP status code:

| Exception Type                              | HTTP Status Code               | Problem Details `type` URI                         |
|---------------------------------------------|--------------------------------|----------------------------------------------------|
| `ValidationException` (FluentValidation)    | `400 Bad Request`              | `"https://api.apfmech.com/errors/validation"`      |
| `DomainException` (custom domain exceptions)| `400 Bad Request`              | `"https://api.apfmech.com/errors/domain-rule"`     |
| `UnauthorizedAccessException`               | `401 Unauthorized`             | `"https://api.apfmech.com/errors/unauthorized"`    |
| `SecurityException` or `ForbiddenException` | `403 Forbidden`                | `"https://api.apfmech.com/errors/forbidden"`       |
| `NotFoundException`                         | `404 Not Found`                | `"https://api.apfmech.com/errors/not-found"`       |
| `DbUpdateConcurrencyException`              | `409 Conflict`                 | `"https://api.apfmech.com/errors/concurrency"`     |
| Any other unhandled exception               | `500 Internal Server Error`    | `"https://api.apfmech.com/errors/internal"`        |

- The `detail` field should contain a human‑readable message derived from the exception (but never the stack trace).
- For validation errors, include an `errors` dictionary mapping field names to error descriptions.
- **Do not** expose internal exception types (e.g., `SqlException`, `NullReferenceException`) in the response.

### 3. Implementation Outline

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Timestamp = DateTimeOffset.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                problemDetails.Type = "https://api.apfmech.com/errors/validation";
                problemDetails.Title = "Validation Error";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationEx.Errors;
                break;
            case DomainException domainEx:
                problemDetails.Type = "https://api.apfmech.com/errors/domain-rule";
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = domainEx.Message;
                break;
            case UnauthorizedAccessException:
                problemDetails.Type = "https://api.apfmech.com/errors/unauthorized";
                problemDetails.Title = "Unauthorized";
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Detail = "Authentication is required.";
                break;
            case ForbiddenException:
                problemDetails.Type = "https://api.apfmech.com/errors/forbidden";
                problemDetails.Title = "Forbidden";
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Detail = "You do not have permission to perform this action.";
                break;
            case NotFoundException:
                problemDetails.Type = "https://api.apfmech.com/errors/not-found";
                problemDetails.Title = "Resource Not Found";
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Detail = exception.Message;
                break;
            default:
                problemDetails.Type = "https://api.apfmech.com/errors/internal";
                problemDetails.Title = "An unexpected error occurred";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "An internal error occurred. Please contact support.";
                break;
        }

        // Log the full exception with structured properties
        _logger.LogError(exception, "HTTP {Method} {Path} failed with status {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            problemDetails.Status);

        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";

        // Optionally add CorrelationId to the response headers
        var correlationId = context.TraceIdentifier;
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
```
- Register the middleware before UseRouting() and UseEndpoints() in Program.cs:
```csharp
    app.UseMiddleware<GlobalExceptionMiddleware>();
```
---
## Structured Logging Standards via Serilog

### 1. Semantic, Structured Logging (No Plain Text)
- **All log messages must use semantic property placeholders** – never embed variables in the message string using concatenation or interpolation.
- **Correct:**
```csharp
Log.Information("WorkOrder {WorkOrderId} created by user {UserId}", workOrderId, currentUserId);
```
- **Incorrect**
```csharp
Log.Information($"WorkOrder {workOrderId} created by user {currentUserId}");
```
- Each property must be a well‑named token (e.g., {WorkOrderId}, {UserId}) so that Serilog can index and query them efficiently.

### 2. Log Levels Guidelines
| Level | Usage |
| :--- | :--- |
| **Verbose** | Detailed debugging traces (only in development, never in production). |
| **Debug** | Diagnostic information helpful for development (e.g., entering a method, parameter values). |
| **Information** | Normal application flow events (e.g., resource created, handler executed). |
| **Warning** | Unusual but handled conditions (e.g., retry attempt, fallback used). |
| **Error** | Exceptions and unexpected failures that are caught and handled (e.g., validation errors, business rule violations). |
| **Fatal** | Catastrophic failures that cause application shutdown (e.g., cannot connect to database). |

- **Never** log sensitive data (passwords, tokens, PII) – mask or omit them.

### 3. Serilog Configuration

- **Sinks:** Use Serilog.Sinks.Console and Serilog.Sinks.File with JSON formatting.
- **Console sink:**
    - In development: use Serilog.Formatting.Compact.CompactJsonFormatter for readable JSON.
    - In production: use CompactJsonFormatter or RenderedCompactJsonFormatter for ingestion by logging aggregators.
- **File sink:**
    - Rolling file with daily or size‑based rolling.
    - Output pure JSON (e.g., using JsonFormatter).
- **Minimum level:** Set to Information in production (Warning or higher for high‑volume systems).
- **Enrichment:**
    - Add WithMachineName(), WithEnvironmentName(), and WithThreadId() for context.
    - Add WithCorrelationId() (see section 4) to include the correlation ID automatically.
Example Program.cs Serilog setup:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)   // Reduce noise
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new JsonFormatter(),
                  "logs/apfmech-.json",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();  // Replace default logging
```
- Ensure appsettings.json also allows overriding via Serilog configuration if needed.

---

## Correlation & Request Tracking

### 1. Correlation ID Generation/Propagation
- **Mandate:** Every incoming HTTP request must have a unique CorrelationId that is either:
    - Read from the X-Correlation-Id request header (if provided by the client).
    - Generated automatically as a new Guid (or Ulid) if the header is missing.
- The correlation ID must be attached to the log context for the entire request lifetime, ensuring that all subsequent log entries (from middleware, handlers, repositories, etc.) include it.
- The response must include the same X-Correlation-Id header for client‑side correlation.
### 2. Implementation Using LogContext.PushProperty
- **In the middleware** (or a separate correlation middleware), push the correlation ID to the Serilog LogContext:
```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue)
            ? headerValue.ToString()
            : Ulid.NewUlid().ToString();   // Or Guid.NewGuid().ToString()

        context.TraceIdentifier = correlationId;   // Built‑in ASP.NET Core field
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```
- Register this middleware before the global exception middleware and any other logging‑sensitive components.

### 3. Logging with Correlation ID
- Once LogContext.PushProperty is used, all subsequent log events automatically include the CorrelationId property.
- Example log output (JSON):
```json
{
  "Timestamp": "2025-01-01T12:00:00.000Z",
  "Level": "Information",
  "MessageTemplate": "WorkOrder {WorkOrderId} created by user {UserId}",
  "Properties": {
    "WorkOrderId": "550e8400-e29b-41d4-a716-446655440000",
    "UserId": "b3a9c5d7-1f2e-4c8b-a8d6-9f7e6a2d1c4b",
    "CorrelationId": "01H0X5Z9KX7M8N2P4Q6R8S9T0",
    "Environment": "Production",
    "MachineName": "APFMech-Server-01"
  }
}
```
### 4. Additional Enrichment
- For database queries, manually push additional context (e.g., tenant ID, user roles) to the LogContext in specific scopes (e.g., within a handler) to further enrich logs.

- Use ILogger<T> with BeginScope to add properties to a block of logs if LogContext is not available.

---

These logging and middleware standards are mandatory for all backend components. Any generated code that produces unstructured logs, leaks stack traces, or omits correlation IDs must be corrected before deployment.