using MedCare.Application.DTOs.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;

        var (statusCode, response) = exception switch
        {
            // Validation exceptions
            ValidationException valEx => (StatusCodes.Status400BadRequest,
                ApiResponse.ValidationFailure(valEx.Errors, valEx.Message)),

            // Not found exceptions
            NotFoundException notFoundEx => (StatusCodes.Status404NotFound,
                ApiResponse.NotFound(notFoundEx.Message)),

            // Business rule violations
            BusinessRuleException bizEx => (StatusCodes.Status400BadRequest,
                ApiResponse.Failure(bizEx.Message, StatusCodes.Status400BadRequest)),

            // Argument validation
            ArgumentException argEx => (StatusCodes.Status400BadRequest,
                ApiResponse.Failure(argEx.Message, StatusCodes.Status400BadRequest)),

            // Invalid operations
            InvalidOperationException invalidOp => (StatusCodes.Status400BadRequest,
                ApiResponse.Failure(invalidOp.Message, StatusCodes.Status400BadRequest)),

            // SQL Server errors
            SqlException sqlEx => HandleSqlException(sqlEx),

            // Optimistic concurrency collision (RowVersion mismatch)
            DBConcurrencyException concEx => (StatusCodes.Status409Conflict,
                ApiResponse.Conflict(concEx.Message)),

            // Authorization failures
            UnauthorizedAccessException unauthEx => (StatusCodes.Status401Unauthorized,
                ApiResponse.Failure(unauthEx.Message, StatusCodes.Status401Unauthorized)),

            // Timeout exceptions
            TimeoutException timeout => (StatusCodes.Status504GatewayTimeout,
                ApiResponse.Failure("The operation timed out. Please try again.",
                    StatusCodes.Status504GatewayTimeout)),

            // File system errors
            IOException ioEx => (StatusCodes.Status500InternalServerError,
                ApiResponse.Failure("A file system error occurred.",
                    StatusCodes.Status500InternalServerError)),

            // Unhandled unexpected internal server error
            _ => (StatusCodes.Status500InternalServerError,
                ApiResponse.Failure(
                    _env.IsDevelopment() ? exception.Message : "An unexpected server error occurred.",
                    StatusCodes.Status500InternalServerError,
                    _env.IsDevelopment() && exception.StackTrace is not null ? [exception.StackTrace] : null))
        };

        // Log with appropriate level
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled server fault [{CorrelationId}]: {Message}",
                correlationId, exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled domain exception [{CorrelationId}]: {Message}",
                correlationId, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers.Append("X-Correlation-ID", correlationId);

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions),
            cancellationToken);

        return true;
    }

    private static (int StatusCode, ApiResponse Response) HandleSqlException(SqlException sqlEx)
    {
        return sqlEx.Number switch
        {
            // 2601 / 2627: Unique constraint / primary key collision
            2601 or 2627 => (StatusCodes.Status409Conflict,
                ApiResponse.Failure(
                    "A record with this identifier already exists (duplicate key violation).",
                    StatusCodes.Status409Conflict)),

            // 515: NULL constraint violation
            515 => (StatusCodes.Status400BadRequest,
                ApiResponse.Failure(
                    "Required field is missing or cannot be NULL.",
                    StatusCodes.Status400BadRequest)),

            // 547: Foreign Key violation (dependent record exists or invalid parent reference)
            547 => (StatusCodes.Status400BadRequest,
                ApiResponse.Failure(
                    "This operation violates data integrity constraints (invalid reference or child records exist).",
                    StatusCodes.Status400BadRequest)),

            // 1205: Deadlock victim
            1205 => (StatusCodes.Status503ServiceUnavailable,
                ApiResponse.Failure(
                    "The server encountered a concurrent lock deadlock. Please retry your request.",
                    StatusCodes.Status503ServiceUnavailable)),

            // Default: Other database errors
            _ => (StatusCodes.Status500InternalServerError,
                ApiResponse.Failure(
                    "A database processing error occurred.",
                    StatusCodes.Status500InternalServerError))
        };
    }
}