namespace Api.Comercial.Models.Responses;

public sealed record ApiResponse<T>(bool Success, string? Message, string? Error, string? TraceId, T? Data);

public static class ApiResponseFactory
{
    public static ApiResponse<T> Ok<T>(T data, string traceId, string? message = null)
        => new(true, message, null, traceId, data);

    public static ApiResponse<T> Fail<T>(string error, string message, string traceId)
        => new(false, message, error, traceId, default);
}

public sealed record OperationResult<T>(bool Success, string? ErrorCode, string? ErrorMessage, T? Data)
{
    public static OperationResult<T> Ok(T data) => new(true, null, null, data);
    public static OperationResult<T> Fail(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage, default);
}
