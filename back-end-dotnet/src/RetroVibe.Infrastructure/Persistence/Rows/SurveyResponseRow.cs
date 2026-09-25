namespace RetroVibe.Infrastructure.Persistence.Rows;

public sealed class SurveyResponseRow
{
    public string Id { get; set; } = null!;
    public string SessionId { get; set; } = null!;
    public string RespondentId { get; set; } = null!;
    public string RespondentRole { get; set; } = null!;
    public int EngagementScore { get; set; }
    public int UsabilityScore { get; set; }
    public string Suggestion { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
}
