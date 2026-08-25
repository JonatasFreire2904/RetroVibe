using System.Text.Json.Serialization;

namespace RetroVibe.Domain.Entities;

public enum AccessLevel
{
    [JsonStringEnumMemberName("ADMIN")] Admin,
    [JsonStringEnumMemberName("FACILITATOR")] Facilitator,
    [JsonStringEnumMemberName("PARTICIPANT")] Participant,
}

public sealed class User
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public string? SquadId { get; private set; }
    public string AvatarColor { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? PasswordHash { get; private set; }
    public AccessLevel AccessLevel { get; private set; }
    public string? AllowedSessionId { get; private set; }

    private User() { }

    public static User Restore(
        string id,
        string name,
        string role,
        string? squadId,
        string avatarColor,
        string? username,
        string? passwordHash,
        AccessLevel accessLevel,
        string? allowedSessionId)
    {
        return new User
        {
            Id = id,
            Name = name,
            Role = role,
            SquadId = squadId,
            AvatarColor = avatarColor,
            Username = username,
            PasswordHash = passwordHash,
            AccessLevel = accessLevel,
            AllowedSessionId = allowedSessionId,
        };
    }

    public bool IsAdmin => AccessLevel == AccessLevel.Admin;
    public bool IsParticipant => AccessLevel == AccessLevel.Participant;

    public bool CanAccessSquad(string squadId)
    {
        if (IsAdmin) return true;
        if (IsParticipant) return false;
        return SquadId == squadId;
    }

    public bool CanAccessSession(string sessionId)
    {
        if (IsAdmin) return true;
        return IsParticipant && AllowedSessionId == sessionId;
    }

    public void UpdateProfile(string name, string role, string? squadId, string avatarColor)
    {
        Name = name;
        Role = role;
        SquadId = squadId;
        AvatarColor = avatarColor;
    }
}
