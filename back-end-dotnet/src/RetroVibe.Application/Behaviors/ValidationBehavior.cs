using FluentValidation;
using MediatR;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next();

        var details = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        var failure = DomainFailure.Validation("Invalid request payload", details);

        var innerType = typeof(TResponse).GetGenericArguments()[0];
        var failMethod = typeof(Result).GetMethod(nameof(Result.Fail))!.MakeGenericMethod(innerType);
        return (TResponse)failMethod.Invoke(null, [failure])!;
    }
}
