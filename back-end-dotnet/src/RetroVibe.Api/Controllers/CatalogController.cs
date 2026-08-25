using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Queries;

namespace RetroVibe.Api.Controllers;

public sealed record CreateTemplateRequest(string? Key, string Label, string Icon, string? Description, List<TemplateColumnDto> Columns);

public sealed record UpdateTemplateRequest(string? Label, string? Icon, string? Description, bool? Active, List<TemplateColumnDto>? Columns);

public sealed record CreateThemeRequest(string? Key, string Label, string Emoji);

public sealed record UpdateThemeRequest(string? Label, string? Emoji, bool? Active);

[ApiController]
[Route("api")]
[Authorize(Policy = "Staff")]
public sealed class CatalogController(IMediator mediator) : ControllerBase
{
    [HttpGet("templates")]
    public async Task<IActionResult> ListTemplates(CancellationToken ct) => Ok(await mediator.Send(new ListTemplatesQuery(), ct));

    [HttpGet("admin/templates")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ListAllTemplates(CancellationToken ct) => Ok(await mediator.Send(new ListAllTemplatesQuery(), ct));

    [HttpPost("admin/templates")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateTemplateCommand(request.Key, request.Label, request.Icon, request.Description, request.Columns), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPatch("admin/templates/{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateTemplate(string id, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateTemplateCommand(id, request.Label, request.Icon, request.Description, request.Active, request.Columns), ct);
        return result.ToActionResult();
    }

    [HttpGet("themes")]
    public async Task<IActionResult> ListThemes(CancellationToken ct) => Ok(await mediator.Send(new ListThemesQuery(), ct));

    [HttpGet("admin/themes")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ListAllThemes(CancellationToken ct) => Ok(await mediator.Send(new ListAllThemesQuery(), ct));

    [HttpPost("admin/themes")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateTheme([FromBody] CreateThemeRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateThemeCommand(request.Key, request.Label, request.Emoji), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPatch("admin/themes/{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateTheme(string id, [FromBody] UpdateThemeRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateThemeCommand(id, request.Label, request.Emoji, request.Active), ct);
        return result.ToActionResult();
    }

    [HttpGet("squads")]
    public async Task<IActionResult> ListSquads(CancellationToken ct) => Ok(await mediator.Send(new ListSquadsQuery(), ct));
}
