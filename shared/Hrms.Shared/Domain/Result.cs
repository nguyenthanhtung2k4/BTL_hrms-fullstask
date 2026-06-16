using System.Collections.Generic;

namespace Hrms.Shared.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string message, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Success(string message = "Operation successful") => new(true, message);
    public static Result Failure(string error, string message = "Operation failed") => new(false, message, new List<string> { error });
    public static Result Failure(List<string> errors, string message = "Operation failed") => new(false, message, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(T? value, bool isSuccess, string message, List<string>? errors = null) 
        : base(isSuccess, message, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string message = "Operation successful") => new(value, true, message);
    public static new Result<T> Failure(string error, string message = "Operation failed") => new(default, false, message, new List<string> { error });
    public static new Result<T> Failure(List<string> errors, string message = "Operation failed") => new(default, false, message, errors);
}
