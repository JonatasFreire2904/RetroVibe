using System.Text.Json.Serialization;

namespace RetroVibe.Domain.Kernel;

public enum FailureKind
{
    [JsonStringEnumMemberName("NOT_FOUND")] NotFound,
    [JsonStringEnumMemberName("VALIDATION")] Validation,
    [JsonStringEnumMemberName("CONFLICT")] Conflict,
    [JsonStringEnumMemberName("FORBIDDEN")] Forbidden,
    [JsonStringEnumMemberName("UNAUTHORIZED")] Unauthorized,
}

public sealed class DomainFailure
{
    public FailureKind Kind { get; }
    public string Message { get; }
    public object? Details { get; }

    private DomainFailure(FailureKind kind, string message, object? details)
    {
        Kind = kind;
        Message = message;
        Details = details;
    }

    public static DomainFailure NotFound(string message, object? details = null) => new(FailureKind.NotFound, message, details);
    public static DomainFailure Validation(string message, object? details = null) => new(FailureKind.Validation, message, details);
    public static DomainFailure Conflict(string message, object? details = null) => new(FailureKind.Conflict, message, details);
    public static DomainFailure Forbidden(string message, object? details = null) => new(FailureKind.Forbidden, message, details);
    public static DomainFailure Unauthorized(string message, object? details = null) => new(FailureKind.Unauthorized, message, details);
}
