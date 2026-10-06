using Microsoft.AspNetCore.Http;
namespace MedCare.Application.DTOs.Common;

public record class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IDictionary<string, string[]>? ValidationErrors { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    // --- Success Factories ---

    public static ApiResponse<T> Success(T data, string? message = null, int statusCode = StatusCodes.Status200OK) =>
        new()
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Created(T data, string? message = "Resource created successfully.") =>
        new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status201Created,
            Message = message,
            Data = data
        };

    // --- Failure Factories ---

    // 1. General business/operation failure (Flat string messages)
    public static ApiResponse<T> Failure(string message, int statusCode = StatusCodes.Status400BadRequest, IReadOnlyList<string>? errors = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? [message]
        };

    // 2. Form/Field validation failure (Dictionary of property-level errors)
    public static ApiResponse<T> ValidationFailure(IDictionary<string, string[]> validationErrors, string message = "One or more validation errors occurred.") =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = message,
            ValidationErrors = validationErrors,
            Errors = validationErrors.SelectMany(kvp => kvp.Value).ToArray()
        };

    // 3. Not Found
    public static ApiResponse<T> NotFound(string message = "Requested resource was not found.") =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status404NotFound,
            Message = message,
            Errors = [message]
        };

    // 4. Concurrency Conflict (Optimistic locking via ROWVERSION)
    public static ApiResponse<T> Conflict(string message = "The record was updated by another user. Please reload.") =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status409Conflict,
            Message = message,
            Errors = [message]
        };
}

// Non-generic variant for commands with no return payload (DELETE, VOID, etc.)
public sealed record ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string? message = null, int statusCode = StatusCodes.Status200OK) =>
        new()
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message
        };

    public static new ApiResponse Failure(string message, int statusCode = StatusCodes.Status400BadRequest, IReadOnlyList<string>? errors = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? [message]
        };

    public static new ApiResponse ValidationFailure(IDictionary<string, string[]> validationErrors, string message = "Validation failed.") =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = message,
            ValidationErrors = validationErrors,
            Errors = validationErrors.SelectMany(kvp => kvp.Value).ToArray()
        };
}
