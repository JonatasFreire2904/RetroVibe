namespace RetroVibe.Application.Dtos;

public sealed record PhaseAveragesDto(int CollectMinutes, int VoteMinutes, int DiscussMinutes);

public sealed record UsageCountDto(string Label, int Count);

public sealed record ParticipantsSeriesPointDto(string SessionLabel, int Participants);

public sealed record TeamDashboardDto(
    double AverageParticipants, double AverageFeedbackScore, int AverageDurationMinutes, int SessionsInPeriod,
    PhaseAveragesDto PhaseAverages, List<UsageCountDto> TemplatesUsage, List<UsageCountDto> ThemesUsage,
    List<ParticipantsSeriesPointDto> ParticipantsSeries);

public sealed record HomeDataDto(List<SessionSummaryDto> RecentSessions, List<TemplateDto> QuickTemplates);
