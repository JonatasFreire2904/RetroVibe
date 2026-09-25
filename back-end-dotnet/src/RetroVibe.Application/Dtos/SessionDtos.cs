using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Dtos;

public sealed record SquadRefDto(string Id, string Name);

public sealed record SessionSummaryDto(
    string Id, string? Title, SessionStatus Status, TemplateDto Template, ThemeDto Theme,
    SquadRefDto Squad, DateTime Date, int ParticipantsCount, int ActionItemsCount, int DurationMinutes);

public sealed record SessionCardDto(
    string Id, string Text, string? AuthorId, bool IsMine, int Votes, int CommentsCount, DateTime CreatedAt);

public sealed record SessionColumnDto(string Id, string Key, string Label, string Icon, List<SessionCardDto> Cards);

public sealed record SessionBoardDto(
    string Id, string? Title, SessionStatus Status, SessionPhase Phase, bool SequentialFlow, int ActiveColumnIndex, PrivacyMode PrivacyMode,
    bool ActionCardsEnabled, bool CardBlurEnabled, bool CardsRevealed, bool SurveyEnabled, bool IsTest,
    TemplateDto Template, ThemeDto Theme, SquadRefDto Squad, DateTime CreatedAt, DateTime? ClosedAt,
    int DurationMinutes, int ParticipantsCount, int ActionItemsCount, List<SessionColumnDto> Columns);
