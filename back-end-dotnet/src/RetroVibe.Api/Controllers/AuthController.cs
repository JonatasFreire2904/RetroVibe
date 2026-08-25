using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Application.Commands;

namespace RetroVibe.Api.Controllers;

public sealed record LoginRequest(string Username, string Password);

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Username, request.Password), ct);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    public IActionResult Logout() => NoContent();
}
