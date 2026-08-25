namespace RetroVibe.Domain.Kernel;

public interface IResult
{
    bool IsOk { get; }
    DomainFailure? Error { get; }
}

public sealed class Result<T> : IResult
{
    public bool IsOk { get; }
    public T? Value { get; }
    public DomainFailure? Error { get; }

    private Result(bool isOk, T? value, DomainFailure? error)
    {
        IsOk = isOk;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(DomainFailure error) => new(false, default, error);

    public static Result<T> FromFailure<TOther>(Result<TOther> failed)
    {
        if (failed.IsOk) throw new InvalidOperationException("Cannot propagate a failure from a successful Result.");
        return Fail(failed.Error!);
    }
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(DomainFailure error) => Result<T>.Fail(error);
}
