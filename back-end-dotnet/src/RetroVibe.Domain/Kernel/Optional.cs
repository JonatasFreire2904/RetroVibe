namespace RetroVibe.Domain.Kernel;

public readonly struct Optional<T>
{
    public bool IsSpecified { get; }
    public T? Value { get; }

    private Optional(bool isSpecified, T? value)
    {
        IsSpecified = isSpecified;
        Value = value;
    }

    public static Optional<T> None => new(false, default);
    public static Optional<T> Of(T? value) => new(true, value);
}
