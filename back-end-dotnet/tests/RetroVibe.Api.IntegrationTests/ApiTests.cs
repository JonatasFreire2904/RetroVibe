using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Dtos;

namespace RetroVibe.Api.IntegrationTests;

public sealed class ApiTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> LoginAsync(string username, string password = "123")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResultDto>(JsonHelper.Options);
        return result!.Token;
    }

    private HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url) { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) } };
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task<SessionBoardDto> CreateSessionAsync(string token, string squadName = "Phoenix", string templateId = "start-stop-continue")
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", token, new { templateId, squadName }));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
    }

    [Fact]
    public async Task RejectsLoginWithWrongPassword()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "marcos", password = "wrong" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RejectsRequestsWithoutBearerToken()
    {
        var response = await _client.GetAsync("/api/home");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatesSessionAsAdminAndReturnsBoard()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token, "Phoenix");

        Assert.Equal("ACTIVE", board.Status.ToString().ToUpperInvariant());
        Assert.Equal("Phoenix", board.Squad.Name);
        Assert.Equal(3, board.Columns.Count);

    }

    [Fact]
    public async Task SequentialRetroAdvancesColumnsForAllParticipantsBeforeVoting()
    {
        var token = await LoginAsync("marcos");
        var create = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", token,
            new { templateId = "start-stop-continue", squadName = "Phoenix", sequentialFlow = true }));
        create.EnsureSuccessStatusCode();
        var board = (await create.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.True(board.SequentialFlow);
        Assert.Equal(0, board.ActiveColumnIndex);

        var futureCard = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token,
            new { columnId = board.Columns[1].Id, text = "Ainda não" }));
        Assert.Equal(HttpStatusCode.Conflict, futureCard.StatusCode);

        var firstCard = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token,
            new { columnId = board.Columns[0].Id, text = "Começar a testar" }));
        Assert.Equal(HttpStatusCode.Created, firstCard.StatusCode);

        var advance = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        advance.EnsureSuccessStatusCode();
        var get = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}", token));
        get.EnsureSuccessStatusCode();
        var current = (await get.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal(1, current.ActiveColumnIndex);
        Assert.Equal(RetroVibe.Domain.Entities.SessionPhase.Collecting, current.Phase);

        var priorCard = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token,
            new { columnId = board.Columns[0].Id, text = "Mais um card" }));
        Assert.Equal(HttpStatusCode.Created, priorCard.StatusCode);

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        var voting = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        voting.EnsureSuccessStatusCode();
        var votingBoard = (await voting.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal(2, votingBoard.ActiveColumnIndex);
        Assert.Equal(RetroVibe.Domain.Entities.SessionPhase.Voting, votingBoard.Phase);
    }

    [Fact]
    public async Task ForcesFacilitatorToCreateSessionsOnlyInOwnSquad()
    {
        var token = await LoginAsync("joao");
        var board = await CreateSessionAsync(token, squadName: "Phoenix");

        Assert.Equal("Cosmos", board.Squad.Name);
    }

    [Fact]
    public async Task BlocksFacilitatorFromViewingAnotherSquadsSession()
    {
        var adminToken = await LoginAsync("marcos");
        var board = await CreateSessionAsync(adminToken, "Phoenix");

        var facilitatorToken = await LoginAsync("joao");
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}", facilitatorToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownSession()
    {
        var token = await LoginAsync("marcos");
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/sessions/does-not-exist", token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddsCardVotesOnItAndClosesSession()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var columnId = board.Columns[0].Id;

        var addCard = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Card 1" }));
        Assert.Equal(HttpStatusCode.Created, addCard.StatusCode);
        var card = (await addCard.Content.ReadFromJsonAsync<SessionCardDto>(JsonHelper.Options))!;

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));

        var vote = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/votes", token, new { cardId = card.Id }));
        var voteResult = await vote.Content.ReadFromJsonAsync<ToggleVoteResultDto>(JsonHelper.Options);
        Assert.True(voteResult!.Voted);
        Assert.Equal(1, voteResult.Votes);

        var close = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/close", token, new { feedbackScore = 4.5 }));
        var closed = (await close.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal("COMPLETED", closed.Status.ToString().ToUpperInvariant());

        var addCardAfterClose = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Too late" }));
        Assert.Equal(HttpStatusCode.Conflict, addCardAfterClose.StatusCode);
    }

    [Fact]
    public async Task CreatesAndUpdatesActionItem()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);

        var create = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/action-items", token, new { sessionId = board.Id, description = "Follow up" }));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var item = (await create.Content.ReadFromJsonAsync<ActionItemDto>(JsonHelper.Options))!;
        Assert.Equal("PLANNED", item.Status.ToString().ToUpperInvariant());

        var update = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/action-items/{item.Id}", token, new { status = "DONE" }));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = (await update.Content.ReadFromJsonAsync<ActionItemDto>(JsonHelper.Options))!;
        Assert.Equal("DONE", updated.Status.ToString().ToUpperInvariant());
    }

    [Fact]
    public async Task ParticipantJoinsActiveSessionWithJustADisplayName()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);

        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Visitante" });
        Assert.Equal(HttpStatusCode.Created, join.StatusCode);
        var joined = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;
        Assert.NotEmpty(joined.Token);
        Assert.Equal("Visitante", joined.Participant.Name);

        var view = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}", joined.Token));
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);
    }

    [Fact]
    public async Task JoinedParticipantCanAddACard()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Visitante" });
        var joined = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;

        var addCard = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/sessions/{board.Id}/cards", joined.Token, new { columnId = board.Columns[0].Id, text = "Contribuição" }));

        Assert.Equal(HttpStatusCode.Created, addCard.StatusCode);
    }

    [Fact]
    public async Task BlocksParticipantFromViewingADifferentSession()
    {
        var token = await LoginAsync("marcos");
        var boardA = await CreateSessionAsync(token);
        var boardB = await CreateSessionAsync(token);

        var join = await _client.PostAsJsonAsync($"/api/sessions/{boardA.Id}/join", new { displayName = "Visitante" });
        var joined = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{boardB.Id}", joined.Token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BlocksParticipantFromStaffOnlyRoutes()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Visitante" });
        var joined = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;

        Assert.Equal(HttpStatusCode.Forbidden, (await _client.SendAsync(Authorized(HttpMethod.Get, "/api/home", joined.Token))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.SendAsync(Authorized(HttpMethod.Get, "/api/sessions", joined.Token))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/sessions", joined.Token, new { templateId = "start-stop-continue", squadName = "Phoenix" }))).StatusCode);
    }

    [Fact]
    public async Task RejectsJoiningASessionThatIsNotActive()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/close", token));

        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Visitante" });
        Assert.Equal(HttpStatusCode.Conflict, join.StatusCode);
    }

    [Fact]
    public async Task OnlyAllowsVotingInVotingAndCardsInCollecting()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var columnId = board.Columns[0].Id;

        var addOk = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Card" }));
        var card = (await addOk.Content.ReadFromJsonAsync<SessionCardDto>(JsonHelper.Options))!;

        var voteTooEarly = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/votes", token, new { cardId = card.Id }));
        Assert.Equal(HttpStatusCode.Conflict, voteTooEarly.StatusCode);

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));

        var addTooLate = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Too late" }));
        Assert.Equal(HttpStatusCode.Conflict, addTooLate.StatusCode);

        var voteOk = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/votes", token, new { cardId = card.Id }));
        Assert.Equal(HttpStatusCode.OK, voteOk.StatusCode);

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        var advancePastLast = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        Assert.Equal(HttpStatusCode.Conflict, advancePastLast.StatusCode);
    }

    [Fact]
    public async Task PausesAndResumesASessionBlockingContributionsWhilePaused()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var columnId = board.Columns[0].Id;

        var pause = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/pause", token));
        var paused = (await pause.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal("PAUSED", paused.Status.ToString().ToUpperInvariant());

        var addWhilePaused = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Nope" }));
        Assert.Equal(HttpStatusCode.Conflict, addWhilePaused.StatusCode);

        var resume = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/resume", token));
        var resumed = (await resume.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal("ACTIVE", resumed.Status.ToString().ToUpperInvariant());

        var addAfterResume = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Now it works" }));
        Assert.Equal(HttpStatusCode.Created, addAfterResume.StatusCode);
    }

    [Fact]
    public async Task LetsTheAuthorEditTheirOwnCardButNotSomeoneElses()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var columnId = board.Columns[0].Id;

        var addCard = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId, text = "Original" }));
        var card = (await addCard.Content.ReadFromJsonAsync<SessionCardDto>(JsonHelper.Options))!;

        var editByAuthor = await _client.SendAsync(Authorized(
            HttpMethod.Patch, $"/api/sessions/{board.Id}/cards/{card.Id}", token, new { text = "Edited" }));
        Assert.Equal(HttpStatusCode.OK, editByAuthor.StatusCode);

        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Outro" });
        var joined = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;

        var editByOther = await _client.SendAsync(Authorized(
            HttpMethod.Patch, $"/api/sessions/{board.Id}/cards/{card.Id}", joined.Token, new { text = "Hijacked" }));
        Assert.Equal(HttpStatusCode.Forbidden, editByOther.StatusCode);
    }

    [Fact]
    public async Task MasksAuthorIdForEveryoneButStillFlagsIsMineForTheAuthor()
    {
        var token = await LoginAsync("marcos");
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/sessions", token, new { templateId = "start-stop-continue", squadName = "Phoenix", privacyMode = "ANONYMOUS" }));
        var board = (await createResponse.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;

        await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token, new { columnId = board.Columns[0].Id, text = "Anônimo" }));

        var reload = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}", token));
        var reloaded = (await reload.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        var card = reloaded.Columns[0].Cards[0];

        Assert.Null(card.AuthorId);
        Assert.True(card.IsMine);
    }

    [Fact]
    public async Task LetsAdminCreateAndDeactivateAThemeButBlocksAFacilitator()
    {
        var facilitatorToken = await LoginAsync("joao");
        var blocked = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/admin/themes", facilitatorToken, new { label = "Copa do Mundo", emoji = "⚽" }));
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);

        var adminToken = await LoginAsync("marcos");
        var create = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/admin/themes", adminToken, new { label = "Copa do Mundo", emoji = "⚽" }));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var theme = (await create.Content.ReadFromJsonAsync<ThemeDto>(JsonHelper.Options))!;
        Assert.True(theme.Active);

        var deactivate = await _client.SendAsync(Authorized(
            HttpMethod.Patch, $"/api/admin/themes/{theme.Id}", adminToken, new { active = false }));
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
        var deactivated = (await deactivate.Content.ReadFromJsonAsync<ThemeDto>(JsonHelper.Options))!;
        Assert.False(deactivated.Active);

        var publicThemes = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/themes", adminToken));
        var themes = (await publicThemes.Content.ReadFromJsonAsync<List<ThemeDto>>(JsonHelper.Options))!;
        Assert.DoesNotContain(themes, t => t.Id == theme.Id);
    }
}
