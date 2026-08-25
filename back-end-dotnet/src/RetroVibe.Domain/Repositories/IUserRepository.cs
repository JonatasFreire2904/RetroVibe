using RetroVibe.Domain.Entities;

namespace RetroVibe.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task SaveAsync(User user, CancellationToken ct = default);
}
