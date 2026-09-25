using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Queries;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "Staff")]
public sealed class DashboardController(IMediator mediator, IUserRepository users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTeamDashboard([FromQuery] string? squadId, CancellationToken ct)
    {
        var account = await users.FindByIdAsync(User.GetUserId(), ct);
        var effectiveSquadId = SquadScope.EffectiveSquadId(User, account, squadId);
        var result = await mediator.Send(new GetTeamDashboardQuery(effectiveSquadId,
            account?.IsTest == true && account.AccessLevel == RetroVibe.Domain.Entities.AccessLevel.Facilitator), ct);
        return Ok(result);
    }
}
