using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task UpdatesSettingsAndNavigatesBackToEarlierStages()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        var settings = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/settings", token,
            new { title = "Sprint 42", privacyMode = "ANONYMOUS", sequentialFlow = true, actionCardsEnabled = false }));
        settings.EnsureSuccessStatusCode();
        var updated = (await settings.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal("Sprint 42", updated.Title);
        Assert.False(updated.ActionCardsEnabled);
        Assert.True(updated.SequentialFlow);

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/phase", token));
        var back = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/stage", token,
            new { phase = "COLLECTING", activeColumnIndex = 0 }));
        back.EnsureSuccessStatusCode();
        var returned = (await back.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.Equal(0, returned.ActiveColumnIndex);
        Assert.Equal(RetroVibe.Domain.Entities.SessionPhase.Collecting, returned.Phase);

        var add = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{board.Id}/cards", token,
            new { columnId = board.Columns[0].Id, text = "Voltei para coleta" }));
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
    }

    [Fact]
    public async Task CardBlurPersistsAndOnlyStaffCanRevealCards()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        Assert.True(board.CardBlurEnabled);
        Assert.False(board.CardsRevealed);

        var join = await _client.PostAsJsonAsync($"/api/sessions/{board.Id}/join", new { displayName = "Visitante" });
        var participant = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;
        var blocked = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/reveal-cards", participant.Token));
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);

        var reveal = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/reveal-cards", token));
        reveal.EnsureSuccessStatusCode();
        var revealed = (await reveal.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.True(revealed.CardsRevealed);

        var reload = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}", participant.Token));
        var participantBoard = (await reload.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.True(participantBoard.CardsRevealed);

        var settings = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/settings", token,
            new { title = "Sprint", privacyMode = "IDENTIFIED", sequentialFlow = false, actionCardsEnabled = true, cardBlurEnabled = false }));
        settings.EnsureSuccessStatusCode();
        var updated = (await settings.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.False(updated.CardBlurEnabled);
    }

    [Fact]
    public async Task ParticipantCanReadOnlyTheirSessionActionItems()
    {
        var token = await LoginAsync("marcos");
        var own = await CreateSessionAsync(token);
        var other = await CreateSessionAsync(token);
        var create = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/action-items", token,
            new { sessionId = own.Id, description = "Definir próximo passo" }));
        create.EnsureSuccessStatusCode();

        var join = await _client.PostAsJsonAsync($"/api/sessions/{own.Id}/join", new { displayName = "Visitante" });
        var participant = (await join.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;
        var ownItems = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{own.Id}/action-items", participant.Token));
        ownItems.EnsureSuccessStatusCode();
        var items = (await ownItems.Content.ReadFromJsonAsync<List<ActionItemDto>>(JsonHelper.Options))!;
        Assert.Contains(items, item => item.Description == "Definir próximo passo");

        var otherItems = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{other.Id}/action-items", participant.Token));
        Assert.Equal(HttpStatusCode.Forbidden, otherItems.StatusCode);
    }

    [Fact]
    public async Task ActionCardsRespectSessionSettingAndAllowChangingAssignee()
    {
        var token = await LoginAsync("marcos");
        var board = await CreateSessionAsync(token);
        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/settings", token,
            new { title = "Ações", privacyMode = "IDENTIFIED", sequentialFlow = false, actionCardsEnabled = false }));

        var blocked = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/action-items", token,
            new { sessionId = board.Id, description = "Ação bloqueada" }));
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{board.Id}/settings", token,
            new { title = "Ações", privacyMode = "IDENTIFIED", sequentialFlow = false, actionCardsEnabled = true }));
        var assigneesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/sessions/{board.Id}/assignees", token));
        assigneesResponse.EnsureSuccessStatusCode();

        var createdResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/action-items", token,
            new { sessionId = board.Id, description = "Ação real" }));
        createdResponse.EnsureSuccessStatusCode();
        var created = (await createdResponse.Content.ReadFromJsonAsync<ActionItemDto>(JsonHelper.Options))!;
        var assignedResponse = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/action-items/{created.Id}", token,
            new { assigneeId = "user-marcos" }));
        assignedResponse.EnsureSuccessStatusCode();
        var assigned = (await assignedResponse.Content.ReadFromJsonAsync<ActionItemDto>(JsonHelper.Options))!;
        Assert.Equal("user-marcos", assigned.Assignee?.Id);

        var summaryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/action-items/sessions-summary", token));
        summaryResponse.EnsureSuccessStatusCode();
        var summaries = (await summaryResponse.Content.ReadFromJsonAsync<List<ActionItemSessionSummaryDto>>(JsonHelper.Options))!;
        Assert.Contains(summaries, summary => summary.SessionId == board.Id && summary.TotalItems == 1 && summary.ThemeIcon == board.Theme.Emoji);
    }

    [Fact]
    public async Task ForcesFacilitatorToCreateSessionsOnlyInOwnSquad()
    {
        var token = await LoginAsync("joao");
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", token,
            new { templateId = "4ls", squadName = "Phoenix" }));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCreatesFacilitatorAndResetsPasswordWithoutExposingIt()
    {
        var admin = await LoginAsync("marcos");
        var existingFacilitator = await LoginAsync("joao");
        var denied = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/admin/facilitators", existingFacilitator,
            new { name = "Intruso", username = "intruso", password = "supersecret123", isTest = false }));
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        var created = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/admin/facilitators", admin,
            new { name = "Luana", username = "luana", password = "initial-secret-123", isTest = true }));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var facilitator = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;
        Assert.DoesNotContain("password", (await created.Content.ReadAsStringAsync()).ToLowerInvariant());
        var previousToken = await LoginAsync("luana", "initial-secret-123");

        var reset = await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/admin/facilitators/{facilitator}/password", admin,
            new { password = "replacement-secret-456" }));
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        var oldPassword = await _client.PostAsJsonAsync("/api/auth/login", new { username = "luana", password = "initial-secret-123" });
        Assert.Equal(HttpStatusCode.BadRequest, oldPassword.StatusCode);
        var oldSession = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/me", previousToken));
        Assert.Equal(HttpStatusCode.Unauthorized, oldSession.StatusCode);
        await LoginAsync("luana", "replacement-secret-456");
    }

    [Fact]
    public async Task TestFacilitatorSeesOwnSurveyAndSessionDashboard()
    {
        var admin = await LoginAsync("marcos");
        var created = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/admin/facilitators", admin,
            new { name = "Facilitadora de teste", username = "teste_dashboard", password = "dashboard-secret-123", isTest = true }));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var facilitator = await LoginAsync("teste_dashboard", "dashboard-secret-123");
        var squad = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/squads", facilitator,
            new { name = "Squad de pesquisa" }));
        Assert.Equal(HttpStatusCode.Created, squad.StatusCode);
        var session = await CreateSessionAsync(facilitator, "Squad de pesquisa", "4ls");
        Assert.True(session.IsTest);
        var joined = await _client.PostAsJsonAsync($"/api/sessions/{session.Id}/join", new { displayName = "Convidado" });
        joined.EnsureSuccessStatusCode();
        var guest = (await joined.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;
        (await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{session.Id}/close", facilitator, new { })))
            .EnsureSuccessStatusCode();
        var answer = new { engagementScore = 5, usabilityScore = 4, suggestion = "Boa experiência" };
        (await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{session.Id}/survey", facilitator, answer)))
            .EnsureSuccessStatusCode();
        (await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{session.Id}/survey", guest.Token, answer)))
            .EnsureSuccessStatusCode();

        var research = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/research/overview?mode=all", facilitator));
        research.EnsureSuccessStatusCode();
        var stats = await research.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, stats.GetProperty("sessions").GetInt32());
        Assert.Equal(2, stats.GetProperty("receivedResponses").GetInt32());
        var dashboard = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/dashboard", facilitator));
        dashboard.EnsureSuccessStatusCode();
        var dashboardStats = await dashboard.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, dashboardStats.GetProperty("sessionsInPeriod").GetInt32());
        var adminDashboard = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/dashboard", admin));
        adminDashboard.EnsureSuccessStatusCode();
        var adminStats = await adminDashboard.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, adminStats.GetProperty("sessionsInPeriod").GetInt32());
    }

    [Fact]
    public async Task FacilitatorCanCreateSeveralSquadsButCannotClaimAnotherThroughProfile()
    {
        var token = await LoginAsync("joao");
        foreach (var name in new[] { "Passarinho", "Dragão" })
        {
            var created = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/squads", token, new { name }));
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }
        var own = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", token,
            new { templateId = "4ls", squadName = "Passarinho" }));
        Assert.Equal(HttpStatusCode.Created, own.StatusCode);

        var profile = await _client.SendAsync(Authorized(HttpMethod.Patch, "/api/me", token,
            new { name = "João", role = "Facilitador", squadName = "Phoenix", avatarColor = "#22C55E" }));
        profile.EnsureSuccessStatusCode();
        var stolen = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", token,
            new { templateId = "4ls", squadName = "Phoenix" }));
        Assert.Equal(HttpStatusCode.Forbidden, stolen.StatusCode);
    }

    [Fact]
    public async Task SurveyRequiresCompletionRejectsDuplicatesAndSeparatesTestSessions()
    {
        var admin = await LoginAsync("marcos");
        var real = await CreateSessionAsync(admin);
        var testCreate = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/sessions", admin,
            new { templateId = "4ls", squadName = "Phoenix", isTest = true }));
        testCreate.EnsureSuccessStatusCode();
        var test = (await testCreate.Content.ReadFromJsonAsync<SessionBoardDto>(JsonHelper.Options))!;
        Assert.True(test.IsTest);

        var joined = await _client.PostAsJsonAsync($"/api/sessions/{real.Id}/join", new { displayName = "Convidada" });
        joined.EnsureSuccessStatusCode();
        var guest = (await joined.Content.ReadFromJsonAsync<JoinSessionResultDto>(JsonHelper.Options))!;
        var card = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/cards", guest.Token,
            new { columnId = real.Columns[0].Id, text = "Aprendizado da equipe" }));
        Assert.Equal(HttpStatusCode.Created, card.StatusCode);
        var answer = new { engagementScore = 4, usabilityScore = 5, suggestion = "Tema alegre" };
        var early = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/survey", guest.Token, answer));
        Assert.Equal(HttpStatusCode.Conflict, early.StatusCode);

        (await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{real.Id}/close", admin, new { }))).EnsureSuccessStatusCode();
        (await _client.SendAsync(Authorized(HttpMethod.Patch, $"/api/sessions/{test.Id}/close", admin, new { }))).EnsureSuccessStatusCode();
        var invalid = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/survey", guest.Token,
            new { engagementScore = 0, usabilityScore = 5, suggestion = "" }));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var guestAnswer = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/survey", guest.Token, answer));
        Assert.Equal(HttpStatusCode.Created, guestAnswer.StatusCode);
        var duplicate = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/survey", guest.Token, answer));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var facilitatorAnswer = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/sessions/{real.Id}/survey", admin,
            new { engagementScore = 3, usabilityScore = 4, suggestion = "" }));
        Assert.Equal(HttpStatusCode.Created, facilitatorAnswer.StatusCode);

        var realOverview = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/research/overview", admin));
        realOverview.EnsureSuccessStatusCode();
        var realStats = await realOverview.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, realStats.GetProperty("sessions").GetInt32());
        Assert.Equal(2, realStats.GetProperty("receivedResponses").GetInt32());
        Assert.Equal(1, realStats.GetProperty("cards").GetInt32());
        var testOverview = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/research/overview?mode=test", admin));
        var testStats = await testOverview.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, testStats.GetProperty("sessions").GetInt32());
        Assert.Equal(0, testStats.GetProperty("receivedResponses").GetInt32());

        var detail = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/research/sessions/{real.Id}", admin));
        detail.EnsureSuccessStatusCode();
        var data = await detail.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, data.GetProperty("responses").GetArrayLength());
        Assert.Equal(2, data.GetProperty("metrics").GetProperty("participants").GetInt32());
        Assert.Equal(1, data.GetProperty("metrics").GetProperty("cards").GetInt32());
        var otherFacilitator = await LoginAsync("joao");
        var denied = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/research/sessions/{real.Id}", otherFacilitator));
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
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
