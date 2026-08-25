using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RetroVibe.Application.Ports;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public string IssueToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(AuthClaimTypes.AccessLevel, user.AccessLevel.ToString()),
        };
        if (user.SquadId is not null) claims.Add(new Claim(AuthClaimTypes.SquadId, user.SquadId));
        if (user.AllowedSessionId is not null) claims.Add(new Claim(AuthClaimTypes.AllowedSessionId, user.AllowedSessionId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
