using FluentValidation;
using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record CreateTemplateCommand(string? Key, string Label, string Icon, string? Description, List<TemplateColumnDto> Columns)
    : IRequest<Result<TemplateDto>>;

public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty();
        RuleFor(x => x.Icon).NotEmpty();
        RuleFor(x => x.Columns).NotEmpty();
    }
}

public sealed class CreateTemplateCommandHandler(ICatalogRepository catalog) : IRequestHandler<CreateTemplateCommand, Result<TemplateDto>>
{
    public async Task<Result<TemplateDto>> Handle(CreateTemplateCommand request, CancellationToken ct)
    {
        var key = string.IsNullOrWhiteSpace(request.Key) ? Slug.From(request.Label) : request.Key;
        var template = await catalog.CreateTemplateAsync(new CreateTemplateInput(
            key, request.Label, request.Icon, request.Description ?? "", IsCustom: true,
            request.Columns.Select(c => new Domain.Entities.RetroColumnBlueprint(c.Key, c.Label, c.Icon)).ToList()), ct);
        return Result<TemplateDto>.Ok(CatalogAssembler.ToTemplateDto(template));
    }
}

public sealed record UpdateTemplateCommand(
    string TemplateId, string? Label, string? Icon, string? Description, bool? Active, List<TemplateColumnDto>? Columns)
    : IRequest<Result<TemplateDto>>;

public sealed class UpdateTemplateCommandHandler(ICatalogRepository catalog) : IRequestHandler<UpdateTemplateCommand, Result<TemplateDto>>
{
    public async Task<Result<TemplateDto>> Handle(UpdateTemplateCommand request, CancellationToken ct)
    {
        var updated = await catalog.UpdateTemplateAsync(request.TemplateId, new UpdateTemplateInput(
            request.Label, request.Icon, request.Description, request.Active,
            request.Columns?.Select(c => new Domain.Entities.RetroColumnBlueprint(c.Key, c.Label, c.Icon)).ToList()), ct);

        return updated is null
            ? Result<TemplateDto>.Fail(DomainFailure.NotFound($"Template {request.TemplateId} not found"))
            : Result<TemplateDto>.Ok(CatalogAssembler.ToTemplateDto(updated));
    }
}

public sealed record CreateThemeCommand(string? Key, string Label, string Emoji) : IRequest<Result<ThemeDto>>;

public sealed class CreateThemeCommandValidator : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty();
        RuleFor(x => x.Emoji).NotEmpty();
    }
}

public sealed class CreateThemeCommandHandler(ICatalogRepository catalog) : IRequestHandler<CreateThemeCommand, Result<ThemeDto>>
{
    public async Task<Result<ThemeDto>> Handle(CreateThemeCommand request, CancellationToken ct)
    {
        var key = string.IsNullOrWhiteSpace(request.Key) ? Slug.From(request.Label) : request.Key;
        var theme = await catalog.CreateThemeAsync(new CreateThemeInput(key, request.Label, request.Emoji), ct);
        return Result<ThemeDto>.Ok(CatalogAssembler.ToThemeDto(theme));
    }
}

public sealed record UpdateThemeCommand(string ThemeId, string? Label, string? Emoji, bool? Active) : IRequest<Result<ThemeDto>>;

public sealed class UpdateThemeCommandHandler(ICatalogRepository catalog) : IRequestHandler<UpdateThemeCommand, Result<ThemeDto>>
{
    public async Task<Result<ThemeDto>> Handle(UpdateThemeCommand request, CancellationToken ct)
    {
        var updated = await catalog.UpdateThemeAsync(request.ThemeId, new UpdateThemeInput(request.Label, request.Emoji, request.Active), ct);
        return updated is null
            ? Result<ThemeDto>.Fail(DomainFailure.NotFound($"Theme {request.ThemeId} not found"))
            : Result<ThemeDto>.Ok(CatalogAssembler.ToThemeDto(updated));
    }
}
