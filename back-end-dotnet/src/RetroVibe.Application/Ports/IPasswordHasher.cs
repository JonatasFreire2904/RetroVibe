namespace RetroVibe.Application.Ports;

public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string storedHash);
}
