using System.Collections.Generic;

namespace Hrms.Shared.Domain;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public ApiResponse() { }

    public ApiResponse(bool success, string message, T? data = default, List<string>? errors = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors ?? new List<string>();
    }

    public static ApiResponse<T> Ok(T data, string message = "Success") => new(true, message, data);
    public static ApiResponse<T> Fail(string error, string message = "Failed") => new(false, message, default, new List<string> { error });
    public static ApiResponse<T> Fail(List<string> errors, string message = "Failed") => new(false, message, default, errors);
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() { }

    public ApiResponse(bool success, string message, object? data = null, List<string>? errors = null)
        : base(success, message, data, errors)
    {
    }

    public static ApiResponse Ok(string message = "Success") => new(true, message);
    public static new ApiResponse Fail(string error, string message = "Failed") => new(false, message, null, new List<string> { error });
    public static new ApiResponse Fail(List<string> errors, string message = "Failed") => new(false, message, null, errors);
}
