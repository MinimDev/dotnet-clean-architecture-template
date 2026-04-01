namespace CleanArchitecture.WebUI.Services.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result<T> Success(T data) => new Result<T> { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error, Errors = new List<string> { error } };
}

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string error) => new Result { IsSuccess = false, Error = error, Errors = new List<string> { error } };
}
