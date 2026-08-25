using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Ports;

public interface IJwtTokenService
{
    string IssueToken(User user);
}
