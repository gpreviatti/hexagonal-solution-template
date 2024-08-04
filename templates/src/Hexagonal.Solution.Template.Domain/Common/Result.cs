namespace Hexagonal.Solution.Template.Domain.Common;
public class Result
{
    protected Result(bool success, string message)
    {
        if (success && message != string.Empty)
            throw new InvalidOperationException();
        
        if (!success && message == string.Empty)
            throw new InvalidOperationException();

        Success = success;

        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
    public bool IsFailure => !Success;

    public static Result Fail(string message) => new(false, "[BusinessError]" + message);

    public static Result<T> Fail<T>(string message) => new(default!, false, "[BusinessError] " + message);

    public static Result Ok() => new(true, string.Empty);

    public static Result<T> Ok<T>(T value) => new(value, true, string.Empty);
}

public sealed class Result<T> : Result
{
    protected internal Result(T value, bool success, string message)
        : base(success, message)
    {
        Value = value;
    }

    public T Value { get; set; }
}
