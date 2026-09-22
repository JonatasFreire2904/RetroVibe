using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Support;

public static class SessionAssembler
{
    public static SessionCardDto ToSessionCardDto(RetroCard card, RetroSession session, string viewerId)
    {
        var isAnonymous = session.PrivacyMode == PrivacyMode.Anonymous;
        return new SessionCardDto(
            card.Id, card.Text, isAnonymous ? null : card.AuthorId, card.AuthorId == viewerId,
            card.VoteCount, card.CommentsCount, card.CreatedAt);
    }

    public static SessionColumnDto ToSessionColumnDto(RetroColumn column, RetroSession session, string viewerId) => new(
        column.Id, column.Key, column.Label, column.Icon,
        column.Cards.Select(c => ToSessionCardDto(c, session, viewerId)).ToList());

    public static SessionSummaryDto ToSessionSummaryDto(
        RetroSession session, TemplateDto template, ThemeDto theme, Squad squad, int actionItemsCount) => new(
        session.Id, session.Title, session.Status, template, theme,
        new SquadRefDto(squad.Id, squad.Name), session.ClosedAt ?? session.CreatedAt,
        session.ParticipantsCount, actionItemsCount, session.DurationMinutes);

    public static SessionBoardDto ToSessionBoardDto(
        RetroSession session, TemplateDto template, ThemeDto theme, Squad squad, int actionItemsCount, string viewerId) => new(
        session.Id, session.Title, session.Status, session.Phase, session.SequentialFlow, session.ActiveColumnIndex,
        session.PrivacyMode, template, theme,
        new SquadRefDto(squad.Id, squad.Name), session.CreatedAt, session.ClosedAt, session.DurationMinutes,
        session.ParticipantsCount, actionItemsCount,
        session.Columns.OrderBy(c => c.Order).Select(c => ToSessionColumnDto(c, session, viewerId)).ToList());
}
