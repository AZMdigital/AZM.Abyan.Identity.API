using AZM.Abyan.Identity.Domain.Enums;

namespace AZM.Abyan.Identity.Application.DTOs.Responses;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public object? Errors { get; init; } 
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public PaginationInfo? Pagination { get; init; }

    // Success variations
    public static Result<T> Success(T data, string message = "")
        => new() { IsSuccess = true, StatusCode = ResultStatus.Success, Data = data, Message = message };
    public static Result<T> Failure(string message = "Operation failed")
    => new() { IsSuccess = false, StatusCode =400, Message = message };
    public static Result<T> Created(T data, string message = "")
        => new() { IsSuccess = true, StatusCode = ResultStatus.Created, Data = data, Message = message };

    public static Result<T> Updated(T data, string message = "")
        => new() { IsSuccess = true, StatusCode = ResultStatus.Updated, Data = data, Message = message };

    public static Result<T> Deleted(T data, string message = "")
        => new() { IsSuccess = true, StatusCode = ResultStatus.Deleted, Data = data, Message = message };

    // Failure variations
    public static Result<T> NotFound(string message = "Resource not found")
        => new() { IsSuccess = false, StatusCode = ResultStatus.NotFound, Message = message };

    public static Result<T> ValidationFailed(object errors, string message = "Validation failed")
        => new() { IsSuccess = false, StatusCode = 400, Message = message, Errors = errors };

    public static Result<T> ValidationFailed(List<string> errors, string message = "Validation failed", T? data = default)
        => new () { IsSuccess = false, StatusCode = 400, Message = message, Errors = errors, Data = data };

    public static Result<T> Unauthorized(string message = "Unauthorized")
        => new() { IsSuccess = false, StatusCode = ResultStatus.Unauthorized, Message = message };

    public static Result<T> Conflict(string message = "Resource conflict")
        => new() { IsSuccess = false, StatusCode = ResultStatus.Conflict, Message = message };
}