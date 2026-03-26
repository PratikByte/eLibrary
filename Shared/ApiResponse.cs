namespace eLibrary.Shared;

/// <summary>
/// Generic API response wrapper for consistent format.
/// </summary>  
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Warning { get; init; }
    public T? Data { get; init; }
    public string[]? Errors { get; init; }

    // Factory helpers
    public static ApiResponse<T> Ok(T data, string? message = null, string? warning = null) =>
        new() { Success = true, Data = data, Message = message, Warning = warning };

    public static ApiResponse<T> Fail(string message, params string[] errors) =>
        new() { Success = false, Message = message, Errors = errors?.Length > 0 ? errors : null };
}

