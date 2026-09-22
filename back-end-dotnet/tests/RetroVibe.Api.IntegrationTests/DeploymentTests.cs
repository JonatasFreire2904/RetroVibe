using System.Net;
using System.Net.Http.Json;

namespace RetroVibe.Api.IntegrationTests;

public sealed class DeploymentTests
{
    [Theory]
    [InlineData("marcos", "test-admin-password-123", "test-facilitator-password-456")]
    [InlineData("joao", "test-facilitator-password-456", "test-admin-password-123")]
    public async Task UsesConfiguredPasswordAndRejectsDemoAndOtherRolePasswords(string username, string password, string otherPassword)
    {
        using var factory = new CustomWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Seed:AdminPassword"] = "test-admin-password-123",
            ["Seed:FacilitatorPassword"] = "test-facilitator-password-456",
        });
        using var client = factory.CreateClient();

        var demoLogin = await client.PostAsJsonAsync("/api/auth/login", new { username, password = "123" });
        Assert.Equal(HttpStatusCode.BadRequest, demoLogin.StatusCode);
        var otherLogin = await client.PostAsJsonAsync("/api/auth/login", new { username, password = otherPassword });
        Assert.Equal(HttpStatusCode.BadRequest, otherLogin.StatusCode);
        var configuredLogin = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        configuredLogin.EnsureSuccessStatusCode();
    }
}
