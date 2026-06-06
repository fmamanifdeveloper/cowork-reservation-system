using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cowork.IntegrationTests.Reservations;

public sealed class ReservationConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReservationConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            "Host=localhost;Port=5433;Database=cowork_reservations;Username=cowork_user;Password=cowork_password"
                    });
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task CreateReservation_ShouldAllowOnlyOneReservation_WhenTwoRequestsOverlapConcurrently()
    {
        var spaceId = await CreateTestSpaceAsync();

        var reservationRequest = new
        {
            spaceId,
            startTime = "2031-01-15T10:00:00Z",
            endTime = "2031-01-15T11:00:00Z"
        };

        var firstRequestTask = _client.PostAsJsonAsync("/api/reservations", reservationRequest);
        var secondRequestTask = _client.PostAsJsonAsync("/api/reservations", reservationRequest);

        var responses = await Task.WhenAll(firstRequestTask, secondRequestTask);

        var statusCodes = responses
            .Select(response => response.StatusCode)
            .ToList();

        statusCodes.Count(status => status == HttpStatusCode.Created)
            .Should()
            .Be(1);

        statusCodes.Count(status => status == HttpStatusCode.Conflict)
            .Should()
            .Be(1);

        var conflictResponse = responses.Single(response =>
            response.StatusCode == HttpStatusCode.Conflict);

        var problem = await conflictResponse.Content.ReadFromJsonAsync<ProblemResponse>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Reservation conflict");
        problem.Detail.Should().Be("The selected time slot is already reserved for this space.");
    }

    private async Task<Guid> CreateTestSpaceAsync()
    {
        var createSpaceRequest = new
        {
            name = $"Concurrency Room {Guid.NewGuid():N}",
            capacity = 4,
            baseHourlyRate = 100m,
            openingTime = "08:00:00",
            closingTime = "20:00:00",
            status = "Active"
        };

        var response = await _client.PostAsJsonAsync("/api/spaces", createSpaceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var space = await response.Content.ReadFromJsonAsync<SpaceResponse>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        space.Should().NotBeNull();

        return space!.Id;
    }

    private sealed record SpaceResponse(Guid Id);

    private sealed record ProblemResponse(
        string? Title,
        string? Detail,
        int? Status);
}