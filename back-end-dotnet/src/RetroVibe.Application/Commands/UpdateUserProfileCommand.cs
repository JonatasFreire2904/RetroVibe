using FluentValidation;
using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record UpdateUserProfileCommand(string UserId, string Name, string Role, string AvatarColor)
    : IRequest<Result<UserDto>>;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.AvatarColor).Must(c => AvatarColors.Palette.Contains(c)).WithMessage("Cor de avatar inválida");
    }
}

public sealed class UpdateUserProfileCommandHandler(IUserRepository users, ICatalogRepository catalog)
    : IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.UserId, ct);
        if (user is null) return Result<UserDto>.Fail(DomainFailure.NotFound($"User {request.UserId} not found"));

        user.UpdateProfile(request.Name, request.Role, request.AvatarColor);
        await users.SaveAsync(user, ct);

        var squad = user.SquadId is null ? null : await catalog.FindSquadByIdAsync(user.SquadId, ct);
        return Result<UserDto>.Ok(new UserDto(user.Id, user.Name, user.Role, squad?.Name, user.AvatarColor, user.AccessLevel, user.IsTest));
    }
}
