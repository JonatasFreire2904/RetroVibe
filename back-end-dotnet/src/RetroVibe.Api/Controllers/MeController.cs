using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Queries;

namespace RetroVibe.Api.Controllers;

public sealed record UpdateProfileRequest(string Name, string? Role, string? SquadName, string AvatarColor);

[ApiController]
[Route("api/me")]
[Authorize(Policy = "Staff")]
public sealed class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var user = await mediator.Send(new GetCurrentUserQuery(User.GetUserId()), ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateUserProfileCommand(User.GetUserId(), request.Name, request.Role ?? "", request.SquadName, request.AvatarColor), ct);
        return result.ToActionResult();
    }
}
