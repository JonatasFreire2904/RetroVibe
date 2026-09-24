using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class RetroSessionConfiguration : IEntityTypeConfiguration<RetroSession>
{
    public void Configure(EntityTypeBuilder<RetroSession> builder)
    {
        builder.ToTable("sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.Title).HasColumnName("title");
        builder.Property(s => s.TemplateId).HasColumnName("template_id").IsRequired();
        builder.Property(s => s.ThemeId).HasColumnName("theme_id").IsRequired();
        builder.Property(s => s.SquadId).HasColumnName("squad_id").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(s => s.Phase).HasColumnName("phase").HasConversion<string>().IsRequired();
        builder.Property(s => s.SequentialFlow).HasColumnName("sequential_flow").HasDefaultValue(false);
        builder.Property(s => s.ActionCardsEnabled).HasColumnName("action_cards_enabled").HasDefaultValue(true);
        builder.Property(s => s.CardBlurEnabled).HasColumnName("card_blur_enabled").HasDefaultValue(false);
        builder.Property(s => s.CardsRevealed).HasColumnName("cards_revealed").HasDefaultValue(false);
        builder.Property(s => s.StageStartedAt).HasColumnName("stage_started_at");
        builder.Property(s => s.CollectSeconds).HasColumnName("collect_seconds").HasDefaultValue(0);
        builder.Property(s => s.VoteSeconds).HasColumnName("vote_seconds").HasDefaultValue(0);
        builder.Property(s => s.DiscussSeconds).HasColumnName("discuss_seconds").HasDefaultValue(0);
        builder.Property(s => s.ActiveColumnIndex).HasColumnName("active_column_index").HasDefaultValue(0);
        builder.Property(s => s.PrivacyMode).HasColumnName("privacy_mode").HasConversion<string>().IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.ClosedAt).HasColumnName("closed_at");
        builder.Property(s => s.ParticipantsCount).HasColumnName("participants_count");
        builder.Property(s => s.FeedbackScore).HasColumnName("feedback_score");

        builder.OwnsOne(s => s.PhaseDurations, pd =>
        {
            pd.Property(p => p.CollectMinutes).HasColumnName("collect_minutes");
            pd.Property(p => p.VoteMinutes).HasColumnName("vote_minutes");
            pd.Property(p => p.DiscussMinutes).HasColumnName("discuss_minutes");
        });

        builder.HasIndex(s => s.SquadId);

        builder.OwnsMany(s => s.Columns, columnBuilder =>
        {
            columnBuilder.ToTable("columns");
            columnBuilder.WithOwner().HasForeignKey("SessionId");
            columnBuilder.HasKey(c => c.Id);
            columnBuilder.Property(c => c.Id).HasColumnName("id");
            columnBuilder.Property(c => c.Key).HasColumnName("key").IsRequired();
            columnBuilder.Property(c => c.Label).HasColumnName("label").IsRequired();
            columnBuilder.Property(c => c.Icon).HasColumnName("icon").IsRequired();
            columnBuilder.Property(c => c.Order).HasColumnName("order_index");
            columnBuilder.Property<string>("SessionId").HasColumnName("session_id");
            columnBuilder.HasIndex("SessionId");

            columnBuilder.OwnsMany(c => c.Cards, cardBuilder =>
            {
                cardBuilder.ToTable("cards");
                cardBuilder.WithOwner().HasForeignKey("ColumnId");
                cardBuilder.HasKey(c => c.Id);
                cardBuilder.Property(c => c.Id).HasColumnName("id");
                cardBuilder.Property(c => c.AuthorId).HasColumnName("author_id").IsRequired();
                cardBuilder.Property(c => c.Text).HasColumnName("text").IsRequired();
                cardBuilder.Property(c => c.CommentsCount).HasColumnName("comments_count");
                cardBuilder.Property(c => c.CreatedAt).HasColumnName("created_at");
                cardBuilder.Property<string>("ColumnId").HasColumnName("column_id");
                cardBuilder.HasIndex("ColumnId");

                cardBuilder.PrimitiveCollection<List<string>>("_voterIds")
                    .HasColumnName("voter_ids");
            });
        });
    }
}
