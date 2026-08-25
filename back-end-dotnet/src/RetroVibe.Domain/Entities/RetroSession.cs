using System.Text.Json.Serialization;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Domain.Entities;

public enum SessionStatus
{
    [JsonStringEnumMemberName("ACTIVE")] Active,
    [JsonStringEnumMemberName("PAUSED")] Paused,
    [JsonStringEnumMemberName("COMPLETED")] Completed,
}

public enum SessionPhase
{
    [JsonStringEnumMemberName("COLLECTING")] Collecting,
    [JsonStringEnumMemberName("VOTING")] Voting,
    [JsonStringEnumMemberName("DISCUSSING")] Discussing,
}

public enum PrivacyMode
{
    [JsonStringEnumMemberName("ANONYMOUS")] Anonymous,
    [JsonStringEnumMemberName("IDENTIFIED")] Identified,
}

public sealed record PhaseDurations(int CollectMinutes, int VoteMinutes, int DiscussMinutes);

public sealed record CloseSessionInput(double? FeedbackScore = null, PhaseDurations? PhaseDurations = null);

public sealed class RetroSession
{
    private static readonly SessionPhase[] PhaseOrder = [SessionPhase.Collecting, SessionPhase.Voting, SessionPhase.Discussing];

    private readonly List<RetroColumn> _columns;

    public string Id { get; private set; } = null!;
    public string? Title { get; private set; }
    public string TemplateId { get; private set; } = null!;
    public string ThemeId { get; private set; } = null!;
    public string SquadId { get; private set; } = null!;
    public SessionStatus Status { get; private set; }
    public SessionPhase Phase { get; private set; }
    public PrivacyMode PrivacyMode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public int ParticipantsCount { get; private set; }
    public double? FeedbackScore { get; private set; }
    public PhaseDurations? PhaseDurations { get; private set; }
    public IReadOnlyList<RetroColumn> Columns => _columns;

    private RetroSession()
    {
        _columns = new List<RetroColumn>();
    }

    public static RetroSession Create(
        string id, string? title, string templateId, string themeId, string squadId,
        PrivacyMode privacyMode, IEnumerable<RetroColumn> columns)
    {
        var session = new RetroSession
        {
            Id = id,
            Title = title,
            TemplateId = templateId,
            ThemeId = themeId,
            SquadId = squadId,
            Status = SessionStatus.Active,
            Phase = SessionPhase.Collecting,
            PrivacyMode = privacyMode,
            CreatedAt = DateTime.UtcNow,
            ClosedAt = null,
            ParticipantsCount = 1,
            FeedbackScore = null,
            PhaseDurations = null,
        };
        session._columns.AddRange(columns);
        return session;
    }

    public static RetroSession Restore(
        string id, string? title, string templateId, string themeId, string squadId,
        SessionStatus status, SessionPhase phase, PrivacyMode privacyMode,
        DateTime createdAt, DateTime? closedAt, int participantsCount,
        double? feedbackScore, PhaseDurations? phaseDurations, IEnumerable<RetroColumn> columns)
    {
        var session = new RetroSession
        {
            Id = id,
            Title = title,
            TemplateId = templateId,
            ThemeId = themeId,
            SquadId = squadId,
            Status = status,
            Phase = phase,
            PrivacyMode = privacyMode,
            CreatedAt = createdAt,
            ClosedAt = closedAt,
            ParticipantsCount = participantsCount,
            FeedbackScore = feedbackScore,
            PhaseDurations = phaseDurations,
        };
        session._columns.AddRange(columns);
        return session;
    }

    public Result<RetroCard> AddCard(string cardId, string columnId, string authorId, string text)
    {
        if (Status != SessionStatus.Active)
        {
            return Result<RetroCard>.Fail(DomainFailure.Conflict("A sessão não está ativa"));
        }
        if (Phase != SessionPhase.Collecting)
        {
            return Result<RetroCard>.Fail(DomainFailure.Conflict("Só é possível adicionar itens na fase de Coleta"));
        }

        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
        {
            return Result<RetroCard>.Fail(DomainFailure.NotFound($"Column {columnId} not found"));
        }

        var created = RetroCard.Create(cardId, columnId, authorId, text);
        if (!created.IsOk) return created;

        column.AddCard(created.Value!);
        return created;
    }

    public Result<RetroCard> EditCard(string cardId, string requestedBy, string text)
    {
        if (Status == SessionStatus.Completed)
        {
            return Result<RetroCard>.Fail(DomainFailure.Conflict("A sessão já foi encerrada"));
        }

        var card = FindCardAnywhere(cardId);
        if (card is null)
        {
            return Result<RetroCard>.Fail(DomainFailure.NotFound($"Card {cardId} not found"));
        }
        if (card.AuthorId != requestedBy)
        {
            return Result<RetroCard>.Fail(DomainFailure.Forbidden("Você só pode editar suas próprias contribuições"));
        }

        var updated = card.UpdateText(text);
        if (!updated.IsOk) return Result<RetroCard>.FromFailure(updated);
        return Result<RetroCard>.Ok(card);
    }

    public Result<bool> ToggleVote(string cardId, string userId)
    {
        if (Status != SessionStatus.Active)
        {
            return Result<bool>.Fail(DomainFailure.Conflict("A sessão não está ativa"));
        }
        if (Phase != SessionPhase.Voting)
        {
            return Result<bool>.Fail(DomainFailure.Conflict("Só é possível votar na fase de Votação"));
        }

        var card = FindCardAnywhere(cardId);
        if (card is null)
        {
            return Result<bool>.Fail(DomainFailure.NotFound($"Card {cardId} not found"));
        }

        return Result<bool>.Ok(card.ToggleVote(userId));
    }

    public Result<Unit> RegisterCardComment(string cardId)
    {
        if (Status != SessionStatus.Active)
        {
            return Result<Unit>.Fail(DomainFailure.Conflict("A sessão não está ativa"));
        }

        var card = FindCardAnywhere(cardId);
        if (card is null)
        {
            return Result<Unit>.Fail(DomainFailure.NotFound($"Card {cardId} not found"));
        }

        card.RegisterComment();
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<SessionPhase> AdvancePhase()
    {
        if (Status != SessionStatus.Active)
        {
            return Result<SessionPhase>.Fail(DomainFailure.Conflict("A sessão não está ativa"));
        }

        var currentIndex = Array.IndexOf(PhaseOrder, Phase);
        if (currentIndex == PhaseOrder.Length - 1)
        {
            return Result<SessionPhase>.Fail(DomainFailure.Conflict("A sessão já está na última fase"));
        }

        Phase = PhaseOrder[currentIndex + 1];
        return Result<SessionPhase>.Ok(Phase);
    }

    public Result<Unit> Pause()
    {
        if (Status != SessionStatus.Active)
        {
            return Result<Unit>.Fail(DomainFailure.Conflict("Só é possível pausar uma sessão ativa"));
        }
        Status = SessionStatus.Paused;
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> Resume()
    {
        if (Status != SessionStatus.Paused)
        {
            return Result<Unit>.Fail(DomainFailure.Conflict("Só é possível retomar uma sessão pausada"));
        }
        Status = SessionStatus.Active;
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> Close(CloseSessionInput? input = null)
    {
        if (Status == SessionStatus.Completed)
        {
            return Result<Unit>.Fail(DomainFailure.Conflict("A sessão já foi encerrada"));
        }
        Status = SessionStatus.Completed;
        ClosedAt = DateTime.UtcNow;
        FeedbackScore = input?.FeedbackScore;
        PhaseDurations = input?.PhaseDurations;
        return Result<Unit>.Ok(Unit.Value);
    }

    public void Join() => ParticipantsCount++;

    public int DurationMinutes => ClosedAt is null ? 0 : (int)Math.Round((ClosedAt.Value - CreatedAt).TotalMinutes);

    public int TotalCards => _columns.Sum(c => c.Cards.Count);

    private RetroCard? FindCardAnywhere(string cardId)
    {
        foreach (var column in _columns)
        {
            var card = column.FindCard(cardId);
            if (card is not null) return card;
        }
        return null;
    }
}
