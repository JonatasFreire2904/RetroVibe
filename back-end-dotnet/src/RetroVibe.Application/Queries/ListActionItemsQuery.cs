using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListActionItemsQuery(string? SessionId, string? SquadId, string? TemplateId, string? ThemeId, ActionItemStatus? Status)
    : IRequest<List<ActionItemDto>>;

public sealed class ListActionItemsQueryHandler(IActionItemRepository actionItems, IUserRepository users)
    : IRequestHandler<ListActionItemsQuery, List<ActionItemDto>>
{
    public async Task<List<ActionItemDto>> Handle(ListActionItemsQuery request, CancellationToken ct)
    {
        var filters = new ActionItemListFilters(request.SessionId, request.SquadId, request.TemplateId, request.ThemeId, request.Status);
        var items = await actionItems.FindAllAsync(filters, ct);

        var dtos = new List<ActionItemDto>();
        foreach (var item in items)
        {
            var assignee = item.AssigneeId is null ? null : await users.FindByIdAsync(item.AssigneeId, ct);
            dtos.Add(ActionItemAssembler.ToActionItemDto(item, assignee));
        }
        return dtos;
    }
}
