using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Queries;

namespace RetroVibe.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "Staff")]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTeamDashboard([FromQuery] string? squadId, CancellationToken ct)
    {
        var effectiveSquadId = SquadScope.EffectiveSquadId(User, squadId);
        var result = await mediator.Send(new GetTeamDashboardQuery(effectiveSquadId), ct);
        return Ok(result);
    }
}
