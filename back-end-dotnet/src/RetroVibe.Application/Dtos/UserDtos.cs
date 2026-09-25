using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Dtos;

public sealed record UserDto(string Id, string Name, string Role, string? Squad, string AvatarColor, AccessLevel AccessLevel, bool IsTest);

public sealed record LoginResultDto(string Token, UserDto User);
