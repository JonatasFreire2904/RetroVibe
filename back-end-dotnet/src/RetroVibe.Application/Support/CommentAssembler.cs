using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Support;

public static class CommentAssembler
{
    public static CommentDto ToCommentDto(Comment comment, User? author, bool mask) => new(
        comment.Id, mask ? null : comment.AuthorId, mask ? null : author?.Name, comment.Text, comment.CreatedAt);
}
