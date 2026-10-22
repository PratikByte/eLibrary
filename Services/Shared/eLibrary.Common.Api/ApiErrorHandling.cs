using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eLibrary.Common.Api;

// Standard JSON shape returned for API failures so clients can handle errors consistently.
public sealed record ApiErrorResponse(
    bool Success,
    string Message,
    string ErrorCode,
    string RequestId,
    string[]? Errors = null)
{
    // Creates an error response using a status-specific code and a safe default message.
    public static ApiErrorResponse ForStatus(int statusCode, string requestId, string? message = null) =>
        new(
            false,
            message ?? GetDefaultMessage(statusCode),
            GetErrorCode(statusCode),
            requestId);

    // Converts HTTP status codes into stable codes that API clients can check.
    internal static string GetErrorCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "VALIDATION_ERROR",
        StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
        StatusCodes.Status403Forbidden => "FORBIDDEN",
        StatusCodes.Status404NotFound => "NOT_FOUND",
        StatusCodes.Status408RequestTimeout => "REQUEST_TIMEOUT",
        StatusCodes.Status409Conflict => "CONFLICT",
        StatusCodes.Status422UnprocessableEntity => "VALIDATION_ERROR",
        StatusCodes.Status429TooManyRequests => "TOO_MANY_REQUESTS",
        StatusCodes.Status504GatewayTimeout => "GATEWAY_TIMEOUT",
        >= 500 => "INTERNAL_SERVER_ERROR",
        _ => "REQUEST_FAILED"
    };

    // Uses user-friendly messages instead of exposing internal exception details.
    internal static string GetDefaultMessage(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Please check the information and try again.",
        StatusCodes.Status401Unauthorized => "Please sign in again to continue.",
        StatusCodes.Status403Forbidden => "You do not have permission to perform this action.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status408RequestTimeout => "The request timed out. Please try again.",
        StatusCodes.Status409Conflict => "The request conflicts with the current data.",
        StatusCodes.Status422UnprocessableEntity => "Please check the information and try again.",
        StatusCodes.Status429TooManyRequests => "Too many requests. Please try again later.",
        StatusCodes.Status504GatewayTimeout => "The service took too long to respond. Please try again.",
        >= 500 => "Something went wrong. Please try again later.",
        _ => "The request could not be completed."
    };
}

public static class ApiErrorHandlingExtensions
{
    // Adds controller filters and a custom response for invalid model state (for example, bad input).
    public static IMvcBuilder AddApiErrorHandling(this IMvcBuilder builder)
    {
        // Normalize controller error results into the shared API error response format.
        builder.AddMvcOptions(options => options.Filters.Add<ApiErrorResultFilter>());
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                // Collect a bounded number of validation messages to keep responses concise.
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        string.IsNullOrWhiteSpace(entry.Key)
                            ? "The request contains an invalid value."
                            : $"{entry.Key}: {error.ErrorMessage}"))
                    .Take(10)
                    .ToArray();

                // Include a request ID in both the response body and header for support and log correlation.
                var requestId = GetRequestId(context.HttpContext);
                SetRequestIdHeader(context.HttpContext, requestId);

                return new BadRequestObjectResult(new ApiErrorResponse(
                    false,
                    "Please check the highlighted fields and try again.",
                    "VALIDATION_ERROR",
                    requestId,
                    errors));
            };
        });
        return builder;
    }

    // Installs runtime middleware for thrown exceptions and status codes with no response body.
    public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
    {
        // Handles exceptions thrown by downstream middleware and endpoints.
        app.UseMiddleware<ApiExceptionMiddleware>();
        // Supplies a standard JSON body when an endpoint sets an error status but writes no body.
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var context = statusCodeContext.HttpContext;
            var requestId = GetRequestId(context);
            SetRequestIdHeader(context, requestId);
            await context.Response.WriteAsJsonAsync(
                ApiErrorResponse.ForStatus(context.Response.StatusCode, requestId),
                context.RequestAborted);
        });

        return app;
    }

    // Prefer the current distributed trace ID; fall back to ASP.NET Core's per-request identifier.
    internal static string GetRequestId(HttpContext context) =>
        Activity.Current?.Id ?? context.TraceIdentifier;

    // Makes the same identifier available to API clients as a response header.
    internal static void SetRequestIdHeader(HttpContext context, string requestId) =>
        context.Response.Headers["X-Request-Id"] = requestId;
}

// Catches exceptions from later middleware/endpoints and converts them into API error responses.
internal sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue through the rest of the HTTP pipeline.
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or cancelled the request; there is no response to complete.
        }
        catch (Exception exception)
        {
            // A response that has started cannot safely be replaced with an error response.
            if (context.Response.HasStarted)
                throw;

            var requestId = ApiErrorHandlingExtensions.GetRequestId(context);
            var (statusCode, message) = MapException(exception);
            // Log server failures with stack traces; expected client errors are logged less noisily.
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled API exception for {Method} {Path}. RequestId: {RequestId}",
                    context.Request.Method, context.Request.Path, requestId);
            }
            else
            {
                _logger.LogInformation("Request failed with {ErrorType} for {Method} {Path}. RequestId: {RequestId}",
                    exception.GetType().Name, context.Request.Method, context.Request.Path, requestId);
            }

            // Discard any partial response and write the normalized error with the mapped HTTP status.
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            ApiErrorHandlingExtensions.SetRequestIdHeader(context, requestId);
            await context.Response.WriteAsJsonAsync(
                ApiErrorResponse.ForStatus(statusCode, requestId, message),
                context.RequestAborted);
        }
    }

    // Maps common exception types to HTTP status codes; unknown exceptions remain server errors.
    private static (int StatusCode, string? Message) MapException(Exception exception) =>
        exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, null),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, null),
            ArgumentException => (StatusCodes.Status400BadRequest, null),
            ValidationException => (StatusCodes.Status400BadRequest, null),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, null),
            _ => (StatusCodes.Status500InternalServerError, null)
        };
}

// Normalizes controller-produced error results, including errors returned inside application envelopes.
internal sealed class ApiErrorResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var originalStatusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
            var isFailureEnvelope = IsFailureEnvelope(objectResult.Value);
            // Handle HTTP errors and success-status responses whose body indicates failure.
            if (originalStatusCode >= StatusCodes.Status400BadRequest || isFailureEnvelope)
            {
                // Preserve known application error codes and map them to matching HTTP statuses.
                var errorCode = GetStringProperty(objectResult.Value, "ErrorCode");
                var mappedStatusCode = GetStatusCode(errorCode);
                var statusCode = isFailureEnvelope
                    ? mappedStatusCode ?? (originalStatusCode < StatusCodes.Status400BadRequest
                        ? StatusCodes.Status500InternalServerError
                        : originalStatusCode)
                    : originalStatusCode;
                var requestId = ApiErrorHandlingExtensions.GetRequestId(context.HttpContext);
                var message = statusCode < StatusCodes.Status500InternalServerError
                    ? GetStringProperty(objectResult.Value, "Message")
                        ?? GetStringProperty(objectResult.Value, "Title")
                    : null;
                var errors = statusCode is StatusCodes.Status400BadRequest or StatusCodes.Status422UnprocessableEntity
                    ? GetValidationErrors(objectResult.Value)
                    : null;

                // Replace the original payload with the shared error contract; hide details for server errors.
                ApiErrorHandlingExtensions.SetRequestIdHeader(context.HttpContext, requestId);
                objectResult.StatusCode = statusCode;
                objectResult.Value = ApiErrorResponse.ForStatus(statusCode, requestId, message) with
                {
                    Errors = errors
                };
                objectResult.DeclaredType = typeof(ApiErrorResponse);
                objectResult.ContentTypes.Clear();
                objectResult.ContentTypes.Add("application/json");
            }
        }

        // Continue executing the MVC result pipeline after any normalization.
        await next();
    }

    // Recognizes the common { Success: false } response envelope without requiring a specific DTO type.
    private static bool IsFailureEnvelope(object? value) =>
        GetProperty(value, "Success") is { } success &&
        success.PropertyType == typeof(bool) &&
        success.GetValue(value) is false;

    // Reads string properties from different response DTOs so they can share this error handling.
    private static string? GetStringProperty(object? value, string name)
    {
        var property = GetProperty(value, name);
        return property?.PropertyType == typeof(string)
            ? property.GetValue(value) as string
            : null;
    }

    // Copies at most ten non-empty validation messages from an application's error envelope.
    private static string[]? GetValidationErrors(object? value)
    {
        var errors = GetProperty(value, "Errors")?.GetValue(value);
        return errors is IEnumerable<string> messages
            ? messages.Where(message => !string.IsNullOrWhiteSpace(message)).Take(10).ToArray()
            : null;
    }

    // Looks up public DTO properties case-insensitively to support existing response naming styles.
    private static PropertyInfo? GetProperty(object? value, string name) =>
        value?.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

    // Translates known application error codes into appropriate HTTP status codes.
    private static int? GetStatusCode(string? errorCode) => errorCode?.ToUpperInvariant() switch
    {
        "NOT_FOUND" or "RESOURCE_NOT_FOUND" or "ENTITY_NOT_FOUND" => StatusCodes.Status404NotFound,
        "CONFLICT" or "DUPLICATE_ENTRY" or "USER_ALREADY_EXISTS" or "ALREADY_RETURNED" => StatusCodes.Status409Conflict,
        "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
        "FORBIDDEN" => StatusCodes.Status403Forbidden,
        "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
        "REQUEST_TIMEOUT" => StatusCodes.Status408RequestTimeout,
        "GATEWAY_TIMEOUT" => StatusCodes.Status504GatewayTimeout,
        "INTERNAL_SERVER_ERROR" => StatusCodes.Status500InternalServerError,
        _ => null
    };
}
