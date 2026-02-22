using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EcoApi.Api.IntegrationTests;

public class WeatherForecastTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();
        var email = $"wf-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "weather-user",
            Email = email,
            Password = "Password123!"
        });
        registerResponse.EnsureSuccessStatusCode();

        using var registerPayload = await JsonDocument.ParseAsync(await registerResponse.Content.ReadAsStreamAsync());
        var accessToken = registerPayload.RootElement.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "ab",
            Email = "invalid-email",
            Password = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Username", out _));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var client = _factory.CreateClient();
        var email = $"refresh-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "refresh-user",
            Email = email,
            Password = "Password123!"
        });
        registerResponse.EnsureSuccessStatusCode();

        using var registerPayload = await JsonDocument.ParseAsync(await registerResponse.Content.ReadAsStreamAsync());
        var initialRefreshToken = registerPayload.RootElement.GetProperty("refreshToken").GetString();

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            RefreshToken = initialRefreshToken
        });

        refreshResponse.EnsureSuccessStatusCode();

        using var refreshPayload = await JsonDocument.ParseAsync(await refreshResponse.Content.ReadAsStreamAsync());
        var newAccessToken = refreshPayload.RootElement.GetProperty("accessToken").GetString();
        var newRefreshToken = refreshPayload.RootElement.GetProperty("refreshToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(newAccessToken));
        Assert.False(string.IsNullOrWhiteSpace(newRefreshToken));
        Assert.NotEqual(initialRefreshToken, newRefreshToken);
    }
}
