namespace eLibrary.Book.Application.Shared;

/// <summary>
/// Shared response wrapper for Book Service API responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message ?? "Operation completed successfully." };

    public static ApiResponse<T> Fail(string message, params string[] errors) =>
        new() { Success = false, Message = message, Errors = errors.Length > 0 ? errors : null };
}
