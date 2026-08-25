using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record GetTeamDashboardQuery(string? SquadId) : IRequest<TeamDashboardDto>;

public sealed class GetTeamDashboardQueryHandler(ISessionRepository sessions, ICatalogRepository catalog)
    : IRequestHandler<GetTeamDashboardQuery, TeamDashboardDto>
{
    public async Task<TeamDashboardDto> Handle(GetTeamDashboardQuery request, CancellationToken ct)
    {
        var all = await sessions.FindAllAsync(new SessionListFilters(request.SquadId), ct);
        var completed = all.Where(s => s.Status == SessionStatus.Completed).ToList();

        var templates = await catalog.ListAllTemplatesIncludingInactiveAsync(ct);
        var themes = await catalog.ListAllThemesIncludingInactiveAsync(ct);
        var templateLabels = templates.ToDictionary(t => t.Id, t => t.Label);
        var themeLabels = themes.ToDictionary(t => t.Id, t => t.Label);

        var averageParticipants = Round1(Average(completed.Select(s => (double)s.ParticipantsCount)));
        var averageFeedbackScore = Round1(Average(completed.Where(s => s.FeedbackScore is not null).Select(s => s.FeedbackScore!.Value)));
        var averageDurationMinutes = (int)Math.Round(Average(completed.Select(s => (double)s.DurationMinutes)));

        var withPhaseDurations = completed.Where(s => s.PhaseDurations is not null).ToList();
        var phaseAverages = new PhaseAveragesDto(
            (int)Math.Round(Average(withPhaseDurations.Select(s => (double)s.PhaseDurations!.CollectMinutes))),
            (int)Math.Round(Average(withPhaseDurations.Select(s => (double)s.PhaseDurations!.VoteMinutes))),
            (int)Math.Round(Average(withPhaseDurations.Select(s => (double)s.PhaseDurations!.DiscussMinutes))));

        var templatesUsage = CountBy(completed.Select(s => templateLabels.GetValueOrDefault(s.TemplateId, "—")));
        var themesUsage = CountBy(completed.Select(s => themeLabels.GetValueOrDefault(s.ThemeId, "—")));

        var participantsSeries = completed
            .OrderBy(s => s.CreatedAt)
            .Select((s, index) => new ParticipantsSeriesPointDto($"#{index + 1}", s.ParticipantsCount))
            .ToList();

        return new TeamDashboardDto(
            averageParticipants, averageFeedbackScore, averageDurationMinutes, completed.Count,
            phaseAverages, templatesUsage, themesUsage, participantsSeries);
    }

    private static double Average(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0 : list.Average();
    }

    private static double Round1(double value) => Math.Round(value, 1);

    private static List<UsageCountDto> CountBy(IEnumerable<string> labels) =>
        labels.GroupBy(l => l)
            .Select(g => new UsageCountDto(g.Key, g.Count()))
            .OrderByDescending(u => u.Count)
            .ToList();
}
