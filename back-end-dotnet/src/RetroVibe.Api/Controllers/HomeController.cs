using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Queries;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Api.Controllers;

[ApiController]
[Route("api/home")]
[Authorize(Policy = "Staff")]
public sealed class HomeController(IMediator mediator, IUserRepository users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHomeData(CancellationToken ct)
    {
        var account = await users.FindByIdAsync(User.GetUserId(), ct);
        var squadId = SquadScope.EffectiveSquadId(User, account, null);
        var data = await mediator.Send(new GetHomeDataQuery(squadId), ct);
        return Ok(data);
    }
}
