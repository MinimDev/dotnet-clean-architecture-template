namespace CleanArchitecture.Application.Common.Models;

/// <summary>
/// Result pattern untuk consistent error handling
/// </summary>
/// <typeparam name="T">Type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, T? data, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error, new List<string> { error });
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, default, errors.FirstOrDefault(), errors);
    }
}

/// <summary>
/// Result pattern tanpa return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error, new List<string> { error });
    }

    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors.FirstOrDefault(), errors);
    }
}
