using Microsoft.EntityFrameworkCore;
using RetroVibe.Application.Ports;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(RetroVibeDbContext db, IPasswordHasher hasher, CancellationToken ct = default, string adminPassword = "123", string facilitatorPassword = "123")
    {
        if (await db.Users.AnyAsync(ct)) return;

        var squads = new[]
        {
            new SquadRow { Id = "phoenix", Name = "Phoenix" },
            new SquadRow { Id = "nebula", Name = "Nebula" },
            new SquadRow { Id = "orion", Name = "Orion" },
            new SquadRow { Id = "cosmos", Name = "Cosmos" },
        };
        db.Squads.AddRange(squads);

        var themes = new[]
        {
            new ThemeRow { Id = "festa-junina", Key = "festa-junina", Label = "Festa Junina", Emoji = "🎉", Active = true },
            new ThemeRow { Id = "natal", Key = "natal", Label = "Natal", Emoji = "🎄", Active = true },
            new ThemeRow { Id = "dia-dos-namorados", Key = "dia-dos-namorados", Label = "Dia dos Namorados", Emoji = "❤️", Active = true },
            new ThemeRow { Id = "pascoa", Key = "pascoa", Label = "Páscoa", Emoji = "🐰", Active = true },
            new ThemeRow { Id = "sem-tema", Key = "sem-tema", Label = "Sem Tema", Emoji = "🗂️", Active = true },
        };
        db.Themes.AddRange(themes);

        var startStopContinue = new TemplateRow
        {
            Id = "start-stop-continue",
            Key = "start-stop-continue",
            Label = "Start / Stop / Continue",
            Icon = "🔄",
            Description = "Identifique o que a equipe deve começar, parar e manter fazendo.",
            IsCustom = false,
            Active = true,
            Columns =
            [
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "start-stop-continue", Key = "start", Label = "Start", Icon = "▶️", OrderIndex = 0 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "start-stop-continue", Key = "stop", Label = "Stop", Icon = "⏹️", OrderIndex = 1 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "start-stop-continue", Key = "continue", Label = "Continue", Icon = "⏩", OrderIndex = 2 },
            ],
        };
        var starfish = new TemplateRow
        {
            Id = "starfish",
            Key = "starfish",
            Label = "Starfish",
            Icon = "⭐",
            Description = "Reflita em cinco dimensões: o que manter, ampliar, reduzir, parar e começar.",
            IsCustom = false,
            Active = true,
            Columns =
            [
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "starfish", Key = "keep", Label = "Continuar", Icon = "✅", OrderIndex = 0 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "starfish", Key = "more", Label = "Mais disso", Icon = "➕", OrderIndex = 1 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "starfish", Key = "less", Label = "Menos disso", Icon = "➖", OrderIndex = 2 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "starfish", Key = "stop", Label = "Parar", Icon = "🛑", OrderIndex = 3 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "starfish", Key = "start", Label = "Começar", Icon = "▶️", OrderIndex = 4 },
            ],
        };
        var fourLs = new TemplateRow
        {
            Id = "4ls",
            Key = "4ls",
            Label = "4Ls",
            Icon = "🧭",
            Description = "Liked, Learned, Lacked, Longed For.",
            IsCustom = false,
            Active = true,
            Columns =
            [
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "4ls", Key = "liked", Label = "Gostei", Icon = "💚", OrderIndex = 0 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "4ls", Key = "learned", Label = "Aprendi", Icon = "💡", OrderIndex = 1 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "4ls", Key = "lacked", Label = "Faltou", Icon = "⚠️", OrderIndex = 2 },
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "4ls", Key = "longed-for", Label = "Desejei", Icon = "🌟", OrderIndex = 3 },
            ],
        };
        var personalizado = new TemplateRow
        {
            Id = "personalizado",
            Key = "personalizado",
            Label = "Personalizado",
            Icon = "🛠️",
            Description = "Modelo customizado criado pelo administrador.",
            IsCustom = true,
            Active = true,
            Columns =
            [
                new TemplateColumnRow { Id = Guid.NewGuid().ToString(), TemplateId = "personalizado", Key = "notas", Label = "Notas", Icon = "📝", OrderIndex = 0 },
            ],
        };
        db.Templates.AddRange(startStopContinue, starfish, fourLs, personalizado);

        var marcos = User.Restore("user-marcos", "Marcos R.", "Administrador", "phoenix", "#7C3AED", "marcos", hasher.Hash(adminPassword), AccessLevel.Admin, null);
        var joao = User.Restore("user-joao", "João", "Facilitador", "cosmos", "#22C55E", "joao", hasher.Hash(facilitatorPassword), AccessLevel.Facilitator, null);
        var ana = User.Restore("user-ana", "Ana", "Facilitadora", "phoenix", "#F97316", null, null, AccessLevel.Facilitator, null);
        var pedro = User.Restore("user-pedro", "Pedro", "Facilitador", "phoenix", "#0EA5E9", null, null, AccessLevel.Facilitator, null);
        var maria = User.Restore("user-maria", "Maria", "Facilitadora", "phoenix", "#EC4899", null, null, AccessLevel.Facilitator, null);
        db.Users.AddRange(marcos, joao, ana, pedro, maria);

        await db.SaveChangesAsync(ct);

        var templates = new[] { startStopContinue, starfish, fourLs };
        var themeIds = new[] { "festa-junina", "natal", "dia-dos-namorados", "pascoa", "sem-tema" };
        var squadIds = new[] { "phoenix", "nebula", "orion", "cosmos" };
        var assignees = new[] { ana.Id, pedro.Id, maria.Id };
        var random = new Random(42);
        var now = DateTime.UtcNow;

        for (var i = 0; i < 10; i++)
        {
            var template = templates[i % templates.Length];
            var daysAgo = 84 + i * 14;
            var createdAt = now.AddDays(-daysAgo);
            var totalMinutes = 25 + random.Next(0, 20);
            var closedAt = createdAt.AddMinutes(totalMinutes);

            var columns = template.Columns
                .OrderBy(c => c.OrderIndex)
                .Select(c => RetroColumn.Create(Guid.NewGuid().ToString(), c.Key, c.Label, c.Icon, c.OrderIndex))
                .ToList();

            var cardCount = 2 + random.Next(0, 5);
            for (var c = 0; c < cardCount; c++)
            {
                var column = columns[random.Next(columns.Count)];
                var cardResult = RetroCard.Create(Guid.NewGuid().ToString(), column.Id, ana.Id, $"Ponto de retrospectiva #{c + 1}");
                var card = cardResult.Value!;
                var voteCount = random.Next(0, 4);
                for (var v = 0; v < voteCount; v++) card.ToggleVote($"voter-{v}");
                if (random.Next(0, 2) == 0) card.RegisterComment();
                column.AddCard(card);
            }

            var session = RetroSession.Restore(
                Guid.NewGuid().ToString(), null, template.Id, themeIds[i % themeIds.Length], squadIds[i % squadIds.Length],
                SessionStatus.Completed, SessionPhase.Discussing, PrivacyMode.Identified,
                createdAt, closedAt, participantsCount: 2 + random.Next(0, 4),
                feedbackScore: Math.Round(3.0 + random.NextDouble() * 2, 1),
                phaseDurations: new PhaseDurations(
                    (int)Math.Round(totalMinutes * 0.35), (int)Math.Round(totalMinutes * 0.26), (int)Math.Round(totalMinutes * 0.39)),
                columns);
            db.Sessions.Add(session);

            var itemCount = 2 + random.Next(0, 5);
            var statuses = Enum.GetValues<ActionItemStatus>();
            for (var a = 0; a < itemCount; a++)
            {
                var status = statuses[random.Next(statuses.Length)];
                var itemResult = ActionItem.Create(
                    Guid.NewGuid().ToString(), session.Id, $"Ação de acompanhamento #{a + 1}",
                    random.Next(0, 2) == 0 ? assignees[random.Next(assignees.Length)] : null,
                    random.Next(0, 2) == 0 ? createdAt.AddDays(14) : null);
                var item = itemResult.Value!;
                item.ChangeStatus(status);
                db.ActionItems.Add(item);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
