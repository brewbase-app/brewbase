using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class TastingSessionEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public TastingSessionEndpointsTests(CoffeeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldCreateTastingSession()
    {
        var sessionName = $"Morning cupping {Guid.NewGuid()}";

        var request = new
        {
            name = sessionName,
            description = "Testing coffees from Ethiopia"
        };

        var response = await _client.PostAsJsonAsync("/api/TastingSessions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        Assert.True(root.GetProperty("id").GetInt32() > 0);
        Assert.Equal(sessionName, root.GetProperty("name").GetString());
        Assert.Equal("Testing coffees from Ethiopia", root.GetProperty("description").GetString());
        Assert.Equal(1, root.GetProperty("userId").GetInt32());
        Assert.True(root.TryGetProperty("createdAt", out _));

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var savedSession = await context.CuppingSessions
            .SingleOrDefaultAsync(session => session.Name == sessionName);

        Assert.NotNull(savedSession);
        Assert.Equal("Testing coffees from Ethiopia", savedSession.Description);
        Assert.Equal(1, savedSession.UserId);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenNameIsEmpty()
    {
        var request = new
        {
            name = "",
            description = "Invalid tasting session"
        };

        var response = await _client.PostAsJsonAsync("/api/TastingSessions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
	
	[Fact]
	public async Task ShouldReturnUserTastingSessions()
	{
    	var firstSessionName = $"User session 1 {Guid.NewGuid()}";
    	var secondSessionName = $"User session 2 {Guid.NewGuid()}";

    	using var scope = _factory.Services.CreateScope();
    	var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    	context.CuppingSessions.AddRange(
        	new CuppingSession
        	{
            	Name = firstSessionName,
            	Description = "First session description",
            	CreatedAt = DateTime.Now,
            	UserId = 1
        	},
        	new CuppingSession
        	{
            	Name = secondSessionName,
            	Description = "Second session description",
            	CreatedAt = DateTime.Now,
            	UserId = 1
        	}
    	);

    	await context.SaveChangesAsync();

   		var response = await _client.GetAsync("/api/TastingSessions");

    	Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    	var payload = await response.Content.ReadAsStringAsync();

    	Assert.Contains(firstSessionName, payload);
    	Assert.Contains(secondSessionName, payload);
	}

	[Fact]
	public async Task ShouldReturnTastingSessionDetailsById()
	{
    	var sessionName = $"Details session {Guid.NewGuid()}";

    	using var scope = _factory.Services.CreateScope();
    	var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    	var session = new CuppingSession
    	{
        	Name = sessionName,
        	Description = "Session details description",
        	CreatedAt = DateTime.Now,
        	UserId = 1
    	};

    	context.CuppingSessions.Add(session);
    	await context.SaveChangesAsync();

    	var response = await _client.GetAsync($"/api/TastingSessions/{session.Id}");

    	Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    	var payload = await response.Content.ReadAsStringAsync();
    	using var document = JsonDocument.Parse(payload);
    	var root = document.RootElement;

    	Assert.Equal(session.Id, root.GetProperty("id").GetInt32());
    	Assert.Equal(sessionName, root.GetProperty("name").GetString());
    	Assert.Equal("Session details description", root.GetProperty("description").GetString());
    	Assert.True(root.TryGetProperty("createdAt", out _));
	}

	[Fact]
	public async Task ShouldReturnNotFoundWhenTastingSessionDoesNotExist()
	{
    	var response = await _client.GetAsync("/api/TastingSessions/999999");

    	Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}