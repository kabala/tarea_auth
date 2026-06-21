using System.Net;
using System.Net.Http.Json;
using AuthApp.Api.Application.Dtos;
using AuthApp.Api.Tests.Infrastructure;

namespace AuthApp.Api.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<AuthAppWebAppFactory>
{
    private readonly AuthAppWebAppFactory _factory;

    public AuthEndpointsTests(AuthAppWebAppFactory factory)
    {
        _factory = factory;
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/antiforgery/token");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
        return content!.Token;
    }

    private static HttpRequestMessage CreateJsonRequest(HttpMethod method, string url, object body, string? xsrfToken = null)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body),
        };

        if (xsrfToken is not null)
        {
            request.Headers.Add("X-XSRF-TOKEN", xsrfToken);
        }

        return request;
    }

    [Fact]
    public async Task GetAntiforgeryToken_Returns200_AndSetsCookies()
    {
        var client = _factory.CreateClientWithCookies();

        var response = await client.GetAsync("/api/antiforgery/token");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(response.Headers.GetValues("Set-Cookie"));
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200_AndSetsAuthCookie()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var request = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "Password123!" }, xsrfToken);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(user);
        Assert.Equal("demo@example.com", user!.Email);
        Assert.Equal("user", user.Role);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var request = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "wrong-password" }, xsrfToken);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingFields_Returns400()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var request = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "", password = "" }, xsrfToken);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_Returns400()
    {
        var client = _factory.CreateClientWithCookies();

        var request = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "Password123!" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClientWithCookies();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithAuth_Returns200_AndUserData()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var loginRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "Password123!" }, xsrfToken);
        await client.SendAsync(loginRequest);

        var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var user = await meResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(user);
        Assert.Equal("demo@example.com", user!.Email);
    }

    [Fact]
    public async Task Logout_WithAuth_Returns200()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var loginRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "Password123!" }, xsrfToken);
        await client.SendAsync(loginRequest);

        var logoutRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/logout", new { }, xsrfToken);
        var logoutResponse = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var logoutRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/logout", new { }, xsrfToken);
        var logoutResponse = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task GetAdmin_WithRegularUser_Returns403()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var loginRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "demo@example.com", password = "Password123!" }, xsrfToken);
        await client.SendAsync(loginRequest);

        var adminResponse = await client.GetAsync("/api/auth/admin");

        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task GetAdmin_WithAdminUser_Returns200()
    {
        var client = _factory.CreateClientWithCookies();
        var xsrfToken = await GetAntiforgeryTokenAsync(client);

        var loginRequest = CreateJsonRequest(HttpMethod.Post, "/api/auth/login",
            new { email = "admin@example.com", password = "Admin123!" }, xsrfToken);
        await client.SendAsync(loginRequest);

        var adminResponse = await client.GetAsync("/api/auth/admin");

        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    private record AntiforgeryTokenResponse(string Token);
}
