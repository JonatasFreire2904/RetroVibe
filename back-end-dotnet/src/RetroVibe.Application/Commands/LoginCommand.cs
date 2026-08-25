using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Ports;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResultDto>>;

public sealed class LoginCommandHandler(
    IUserRepository users, ICatalogRepository catalog, IPasswordHasher hasher, IJwtTokenService tokens)
    : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private const string InvalidCredentialsMessage = "Usuário ou senha inválidos";

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await users.FindByUsernameAsync(username, ct);
        if (user is null || user.PasswordHash is null || !hasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResultDto>.Fail(DomainFailure.Validation(InvalidCredentialsMessage));
        }

        var token = tokens.IssueToken(user);
        var squad = user.SquadId is null ? null : await catalog.FindSquadByIdAsync(user.SquadId, ct);

        var userDto = new UserDto(user.Id, user.Name, user.Role, squad?.Name, user.AvatarColor, user.AccessLevel);
        return Result<LoginResultDto>.Ok(new LoginResultDto(token, userDto));
    }
}
