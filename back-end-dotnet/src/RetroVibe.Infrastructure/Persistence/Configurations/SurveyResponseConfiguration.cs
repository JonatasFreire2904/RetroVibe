using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponseRow>
{
    public void Configure(EntityTypeBuilder<SurveyResponseRow> builder)
    {
        builder.ToTable("survey_responses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(r => r.RespondentId).HasColumnName("respondent_id").IsRequired();
        builder.Property(r => r.RespondentRole).HasColumnName("respondent_role").IsRequired();
        builder.Property(r => r.EngagementScore).HasColumnName("engagement_score");
        builder.Property(r => r.UsabilityScore).HasColumnName("usability_score");
        builder.Property(r => r.Suggestion).HasColumnName("suggestion").IsRequired();
        builder.Property(r => r.SubmittedAt).HasColumnName("submitted_at");
        builder.HasIndex(r => new { r.SessionId, r.RespondentId }).IsUnique();
    }
}
