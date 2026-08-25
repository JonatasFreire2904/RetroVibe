using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record CreateSessionCommand(
    string? Title, string TemplateId, string? ThemeId, string SquadName, PrivacyMode? PrivacyMode, string RequestedBy)
    : IRequest<Result<SessionBoardDto>>;

public sealed class CreateSessionCommandHandler(ISessionRepository sessions, ICatalogRepository catalog, IUserRepository users)
    : IRequestHandler<CreateSessionCommand, Result<SessionBoardDto>>
{
    public async Task<Result<SessionBoardDto>> Handle(CreateSessionCommand request, CancellationToken ct)
    {
        var requester = await users.FindByIdAsync(request.RequestedBy, ct);
        if (requester is null)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));
        }
        if (requester.IsParticipant)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.Forbidden("Ação não disponível para participantes"));
        }

        string squadNameInput;
        if (requester.IsAdmin)
        {
            squadNameInput = request.SquadName.Trim();
        }
        else
        {
            if (requester.SquadId is null)
            {
                return Result<SessionBoardDto>.Fail(DomainFailure.Validation("Seu usuário não está associado a nenhum squad"));
            }
            var ownSquad = await catalog.FindSquadByIdAsync(requester.SquadId, ct);
            squadNameInput = ownSquad?.Name ?? "";
        }
        if (squadNameInput.Length == 0)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.Validation("O nome do squad é obrigatório"));
        }

        var template = await catalog.FindTemplateByIdAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.NotFound($"Template {request.TemplateId} not found"));
        }
        if (!template.Active)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.Conflict("Este modelo foi desativado pelo administrador"));
        }

        var themeId = string.IsNullOrWhiteSpace(request.ThemeId) ? "sem-tema" : request.ThemeId;
        var theme = await catalog.FindThemeByIdAsync(themeId, ct);
        if (theme is null)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.NotFound($"Theme {themeId} not found"));
        }
        if (!theme.Active)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.Conflict("Este tema foi desativado pelo administrador"));
        }

        var squad = await catalog.FindOrCreateSquadByNameAsync(squadNameInput, ct);

        var columns = template.Columns
            .Select((c, index) => RetroColumn.Create(Guid.NewGuid().ToString(), c.Key, c.Label, c.Icon, index))
            .ToList();

        var session = RetroSession.Create(
            Guid.NewGuid().ToString(), request.Title, template.Id, theme.Id, squad.Id,
            request.PrivacyMode ?? Domain.Entities.PrivacyMode.Identified, columns);

        await sessions.SaveAsync(session, ct);

        return Result<SessionBoardDto>.Ok(SessionAssembler.ToSessionBoardDto(
            session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, 0, requester.Id));
    }
}
