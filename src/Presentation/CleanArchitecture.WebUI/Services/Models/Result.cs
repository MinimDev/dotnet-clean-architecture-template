namespace CleanArchitecture.WebUI.Services.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string error) => new Result { IsSuccess = false, Error = error, Errors = new List<string> { error } };
}
