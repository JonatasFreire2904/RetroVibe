using System.Text.Json.Serialization;

namespace RetroVibe.Domain.Entities;

public enum AccessLevel
{
    [JsonStringEnumMemberName("ADMIN")] Admin,
    [JsonStringEnumMemberName("FACILITATOR")] Facilitator,
    [JsonStringEnumMemberName("PARTICIPANT")] Participant,
    [JsonStringEnumMemberName("MEMBER")] Member,
}

public sealed class User
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public string? SquadId { get; private set; }
    public List<string> ManagedSquadIds { get; private set; } = [];
    public bool IsTest { get; private set; }
    public int TokenVersion { get; private set; }
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
        string? allowedSessionId,
        bool isTest = false,
        IEnumerable<string>? managedSquadIds = null)
    {
        return new User
        {
            Id = id,
            Name = name,
            Role = role,
            SquadId = squadId,
            ManagedSquadIds = managedSquadIds?.Distinct().ToList() ??
                (accessLevel == AccessLevel.Facilitator && squadId is not null ? [squadId] : []),
            IsTest = isTest,
            AvatarColor = avatarColor,
            Username = username,
            PasswordHash = passwordHash,
            AccessLevel = accessLevel,
            AllowedSessionId = allowedSessionId,
        };
    }

    public static User CreateSquadMember(string id, string name, string squadId, string avatarColor) =>
        Restore(id, name, "Membro", squadId, avatarColor, username: null, passwordHash: null, AccessLevel.Member, allowedSessionId: null);

    public bool IsAdmin => AccessLevel == AccessLevel.Admin;
    public bool IsParticipant => AccessLevel == AccessLevel.Participant;
    public bool IsSquadMember => AccessLevel == AccessLevel.Member;

    public bool CanAccessSquad(string squadId)
    {
        if (IsAdmin) return true;
        if (IsParticipant || IsSquadMember) return false;
        return SquadId == squadId || ManagedSquadIds.Contains(squadId);
    }

    public bool CanBeAssignedInSquad(string squadId) =>
        IsSquadMember || IsAdmin ? SquadId == squadId : CanAccessSquad(squadId);

    public bool CanAccessSession(string sessionId)
    {
        if (IsAdmin) return true;
        return IsParticipant && AllowedSessionId == sessionId;
    }

    public void UpdateProfile(string name, string role, string avatarColor)
    {
        Name = name;
        Role = role;
        AvatarColor = avatarColor;
    }

    public void AddManagedSquad(string squadId)
    {
        if (!ManagedSquadIds.Contains(squadId)) ManagedSquadIds.Add(squadId);
        SquadId ??= squadId;
    }

    public void ResetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        TokenVersion++;
    }

    public void SetTest(bool isTest) => IsTest = isTest;

    public void LeaveSquad() => SquadId = null;
}
