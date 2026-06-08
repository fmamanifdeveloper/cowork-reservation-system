using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cowork.IntegrationTests.Reservations;

public sealed class ReservationConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReservationConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateReservation_ShouldAllowOnlyOneReservation_WhenTwoRequestsOverlapConcurrently()
    {
        var token = await LoginAsAdminAsync();

        var spaceId = await CreateTestSpaceAsync(token);
        var firstCustomerId = await CreateTestCustomerAsync(token, "first");
        var secondCustomerId = await CreateTestCustomerAsync(token, "second");

        var startTime = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.FromHours(-5));
        var firstEndTime = new DateTimeOffset(2026, 7, 10, 12, 0, 0, TimeSpan.FromHours(-5));

        var secondStartTime = new DateTimeOffset(2026, 7, 10, 10, 30, 0, TimeSpan.FromHours(-5));
        var secondEndTime = new DateTimeOffset(2026, 7, 10, 11, 30, 0, TimeSpan.FromHours(-5));

        using var firstRequest = CreateAuthorizedJsonRequest(
            HttpMethod.Post,
            "/api/reservations",
            token,
            new CreateReservationRequest(
                spaceId,
                firstCustomerId,
                startTime,
                firstEndTime));

        using var secondRequest = CreateAuthorizedJsonRequest(
            HttpMethod.Post,
            "/api/reservations",
            token,
            new CreateReservationRequest(
                spaceId,
                secondCustomerId,
                secondStartTime,
                secondEndTime));

        var responses = await Task.WhenAll(
            _client.SendAsync(firstRequest),
            _client.SendAsync(secondRequest));

        var statusCodes = responses
            .Select(response => response.StatusCode)
            .ToList();

        Assert.Contains(HttpStatusCode.Created, statusCodes);
        Assert.Contains(HttpStatusCode.Conflict, statusCodes);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin", "Admin123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse!.AccessToken));

        return loginResponse.AccessToken;
    }

    private async Task<Guid> CreateTestSpaceAsync(string token)
    {
        using var request = CreateAuthorizedJsonRequest(
            HttpMethod.Post,
            "/api/spaces",
            token,
            new CreateSpaceRequest(
                $"Sala Concurrencia {Guid.NewGuid():N}",
                10,
                50m,
                "08:00:00",
                "22:00:00",
                "America/Lima",
                "Active"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var space = await response.Content.ReadFromJsonAsync<SpaceResponse>();

        Assert.NotNull(space);

        return space!.Id;
    }

    private async Task<Guid> CreateTestCustomerAsync(string token, string suffix)
    {
        var uniqueValue = Guid.NewGuid().ToString("N");

        using var request = CreateAuthorizedJsonRequest(
            HttpMethod.Post,
            "/api/customers",
            token,
            new CreateCustomerRequest(
                $"Cliente Integración {suffix} {uniqueValue}",
                $"integration.{suffix}.{uniqueValue}@example.com",
                "999888777",
                uniqueValue[..8]));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.NotNull(customer);

        return customer!.Id;
    }

    private static HttpRequestMessage CreateAuthorizedJsonRequest<T>(
        HttpMethod method,
        string url,
        string token,
        T body)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    private sealed record LoginRequest(
        string Username,
        string Password);

    private sealed record LoginResponse(
        string AccessToken,
        AuthenticatedUser User);

    private sealed record AuthenticatedUser(
        Guid Id,
        Guid? CustomerId,
        string Username,
        string DisplayName,
        string Role);

    private sealed record CreateSpaceRequest(
        string Name,
        int Capacity,
        decimal BaseHourlyRate,
        string OpeningTime,
        string ClosingTime,
        string TimeZoneId,
        string Status);

    private sealed record SpaceResponse(
        Guid Id,
        string Name,
        int Capacity,
        decimal BaseHourlyRate,
        string OpeningTime,
        string ClosingTime,
        string TimeZoneId,
        string Status);

    private sealed record CreateCustomerRequest(
        string FullName,
        string? Email,
        string? Phone,
        string? DocumentNumber);

    private sealed record CustomerResponse(
        Guid Id,
        string FullName,
        string? Email,
        string? Phone,
        string? DocumentNumber);

    private sealed record CreateReservationRequest(
        Guid SpaceId,
        Guid CustomerId,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime);
}