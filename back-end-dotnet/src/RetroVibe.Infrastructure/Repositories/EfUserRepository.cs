using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Infrastructure.Repositories;

public sealed class EfUserRepository(RetroVibeDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(string id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<IReadOnlyList<User>> ListBySquadAsync(string squadId, CancellationToken ct = default) =>
        await db.Users.Where(u => u.SquadId == squadId && u.AccessLevel != AccessLevel.Participant)
            .OrderBy(u => u.Name).ToListAsync(ct);

    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        if (db.Entry(user).State == EntityState.Detached)
        {
            var exists = await db.Users.AnyAsync(u => u.Id == user.Id, ct);
            if (exists) db.Users.Update(user);
            else db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}
