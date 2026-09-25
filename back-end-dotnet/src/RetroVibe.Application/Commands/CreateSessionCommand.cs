using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record CreateSessionCommand(
    string? Title, string TemplateId, string? ThemeId, string SquadName, PrivacyMode? PrivacyMode,
    bool? SequentialFlow, bool? ActionCardsEnabled, bool? CardBlurEnabled, bool? IsTest, string RequestedBy)
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

        var squadNameInput = request.SquadName.Trim();
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

        var availableSquads = await catalog.ListSquadsAsync(ct);
        var squad = availableSquads.FirstOrDefault(s => string.Equals(s.Name, squadNameInput, StringComparison.OrdinalIgnoreCase));
        if (squad is null || !requester.CanAccessSquad(squad.Id))
            return Result<SessionBoardDto>.Fail(DomainFailure.Forbidden("Selecione um squad associado à sua conta"));

        var columns = template.Columns
            .Select((c, index) => RetroColumn.Create(Guid.NewGuid().ToString(), c.Key, c.Label, c.Icon, index))
            .ToList();

        var session = RetroSession.Create(
            Guid.NewGuid().ToString(), request.Title, template.Id, theme.Id, squad.Id,
            request.PrivacyMode ?? Domain.Entities.PrivacyMode.Identified, columns, request.SequentialFlow ?? false,
            request.ActionCardsEnabled ?? true, request.CardBlurEnabled ?? true,
            requester.Id, requester.IsTest || request.IsTest == true, surveyEnabled: true);

        await sessions.SaveAsync(session, ct);

        return Result<SessionBoardDto>.Ok(SessionAssembler.ToSessionBoardDto(
            session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, 0, requester.Id));
    }
}
