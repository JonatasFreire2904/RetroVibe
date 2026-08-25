using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListTemplatesQuery : IRequest<List<TemplateDto>>;

public sealed class ListTemplatesQueryHandler(ICatalogRepository catalog) : IRequestHandler<ListTemplatesQuery, List<TemplateDto>>
{
    public async Task<List<TemplateDto>> Handle(ListTemplatesQuery request, CancellationToken ct) =>
        (await catalog.ListTemplatesAsync(ct)).Select(CatalogAssembler.ToTemplateDto).ToList();
}

public sealed record ListAllTemplatesQuery : IRequest<List<TemplateDto>>;

public sealed class ListAllTemplatesQueryHandler(ICatalogRepository catalog) : IRequestHandler<ListAllTemplatesQuery, List<TemplateDto>>
{
    public async Task<List<TemplateDto>> Handle(ListAllTemplatesQuery request, CancellationToken ct) =>
        (await catalog.ListAllTemplatesIncludingInactiveAsync(ct)).Select(CatalogAssembler.ToTemplateDto).ToList();
}

public sealed record ListThemesQuery : IRequest<List<ThemeDto>>;

public sealed class ListThemesQueryHandler(ICatalogRepository catalog) : IRequestHandler<ListThemesQuery, List<ThemeDto>>
{
    public async Task<List<ThemeDto>> Handle(ListThemesQuery request, CancellationToken ct) =>
        (await catalog.ListThemesAsync(ct)).Select(CatalogAssembler.ToThemeDto).ToList();
}

public sealed record ListAllThemesQuery : IRequest<List<ThemeDto>>;

public sealed class ListAllThemesQueryHandler(ICatalogRepository catalog) : IRequestHandler<ListAllThemesQuery, List<ThemeDto>>
{
    public async Task<List<ThemeDto>> Handle(ListAllThemesQuery request, CancellationToken ct) =>
        (await catalog.ListAllThemesIncludingInactiveAsync(ct)).Select(CatalogAssembler.ToThemeDto).ToList();
}

public sealed record ListSquadsQuery : IRequest<List<SquadDto>>;

public sealed class ListSquadsQueryHandler(ICatalogRepository catalog) : IRequestHandler<ListSquadsQuery, List<SquadDto>>
{
    public async Task<List<SquadDto>> Handle(ListSquadsQuery request, CancellationToken ct) =>
        (await catalog.ListSquadsAsync(ct)).Select(CatalogAssembler.ToSquadDto).ToList();
}
