using Microsoft.AspNetCore.Identity;
using RetroVibe.Application.Ports;

namespace RetroVibe.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string plainPassword) => _inner.HashPassword(new object(), plainPassword);

    public bool Verify(string plainPassword, string storedHash)
    {
        var result = _inner.VerifyHashedPassword(new object(), storedHash, plainPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
