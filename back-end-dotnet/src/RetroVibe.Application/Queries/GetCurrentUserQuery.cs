using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record GetCurrentUserQuery(string UserId) : IRequest<UserDto?>;

public sealed class GetCurrentUserQueryHandler(IUserRepository users, ICatalogRepository catalog)
    : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.UserId, ct);
        if (user is null) return null;

        var squad = user.SquadId is null ? null : await catalog.FindSquadByIdAsync(user.SquadId, ct);
        return new UserDto(user.Id, user.Name, user.Role, squad?.Name, user.AvatarColor, user.AccessLevel, user.IsTest);
    }
}
