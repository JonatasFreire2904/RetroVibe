namespace RetroVibe.Application.Dtos;

public sealed record CommentDto(string Id, string? AuthorId, string? AuthorName, string Text, DateTime CreatedAt);
