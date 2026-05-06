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
		_client = _factory.CreateAuthenticatedClient();    
	}

    [Fact]
    public async Task ShouldCreateTastingSession()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

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
        var otherUserSessionName = $"Other user session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

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
            },
            new CuppingSession
            {
                Name = otherUserSessionName,
                Description = "Other user session description",
                CreatedAt = DateTime.Now,
                UserId = 2
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/TastingSessions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();

        Assert.Contains(firstSessionName, payload);
        Assert.Contains(secondSessionName, payload);
        Assert.DoesNotContain(otherUserSessionName, payload);
    }

    [Fact]
    public async Task ShouldReturnTastingSessionDetailsById()
    {
        var sessionName = $"Details session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

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

    [Fact]
    public async Task ShouldIgnoreUserIdAndCreatedAtFromRequestBody()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var sessionName = $"Controlled fields session {Guid.NewGuid()}";

        var request = new
        {
            name = sessionName,
            description = "Session with ignored fields",
            userId = 999,
            createdAt = new DateTime(2000, 1, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/TastingSessions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var savedSession = await context.CuppingSessions
            .SingleAsync(session => session.Name == sessionName);

        Assert.Equal(1, savedSession.UserId);
        Assert.NotEqual(new DateTime(2000, 1, 1), savedSession.CreatedAt);
    }
	
	[Fact]
    public async Task ShouldAddCoffeeToTastingSession()
    {
        var sessionName = $"Add coffee session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var session = new CuppingSession
        {
            Name = sessionName,
            Description = "Session for adding coffee",
            CreatedAt = DateTime.Now,
            UserId = 1
        };

        context.CuppingSessions.Add(session);
        await context.SaveChangesAsync();

        var request = new
        {
            coffeeId = 1,
			notes = "Bright acidity and floral aroma"
        };

        var response = await _client.PostAsJsonAsync($"/api/TastingSessions/{session.Id}/coffees", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("coffeeId").GetInt32());
		Assert.Equal("Alpha Coffee", root.GetProperty("coffeeName").GetString());
        Assert.Equal("Bright acidity and floral aroma", root.GetProperty("notes").GetString());

        var savedSessionCoffee = await context.CuppingSessionCoffees
            .SingleOrDefaultAsync(sessionCoffee =>
                sessionCoffee.CuppingSessionId == session.Id &&
                sessionCoffee.CoffeeId == 1);

        Assert.NotNull(savedSessionCoffee);
		Assert.Equal("Bright acidity and floral aroma", savedSessionCoffee.Notes);

        var detailsResponse = await _client.GetAsync($"/api/TastingSessions/{session.Id}");

        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);

        var detailsPayload = await detailsResponse.Content.ReadAsStringAsync();
        using var detailsDocument = JsonDocument.Parse(detailsPayload);
        var coffees = detailsDocument.RootElement.GetProperty("coffees");

        Assert.Contains(coffees.EnumerateArray(), coffee =>
            coffee.GetProperty("coffeeId").GetInt32() == 1);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenAddingMissingCoffeeToTastingSession()
    {
        var sessionName = $"Missing coffee session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var session = new CuppingSession
        {
            Name = sessionName,
            Description = "Session for missing coffee",
            CreatedAt = DateTime.Now,
            UserId = 1
        };

        context.CuppingSessions.Add(session);
        await context.SaveChangesAsync();

        var request = new
        {
            coffeeId = 999999
        };

        var response = await _client.PostAsJsonAsync($"/api/TastingSessions/{session.Id}/coffees", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenAddingCoffeeToMissingTastingSession()
    {
        var request = new
        {
            coffeeId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/TastingSessions/999999/coffees", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnConflictWhenAddingSameCoffeeTwiceToTastingSession()
    {
        var sessionName = $"Duplicate coffee session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var session = new CuppingSession
        {
            Name = sessionName,
            Description = "Session for duplicate coffee",
            CreatedAt = DateTime.Now,
            UserId = 1
        };

        context.CuppingSessions.Add(session);
        await context.SaveChangesAsync();

        context.CuppingSessionCoffees.Add(new CuppingSessionCoffee
        {
            CuppingSessionId = session.Id,
            CoffeeId = 1,
            CreatedAt = DateTime.Now
        });

        await context.SaveChangesAsync();

        var request = new
        {
            coffeeId = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/TastingSessions/{session.Id}/coffees", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var count = await context.CuppingSessionCoffees
            .CountAsync(sessionCoffee =>
                sessionCoffee.CuppingSessionId == session.Id &&
                sessionCoffee.CoffeeId == 1);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenAddingCoffeeToOtherUserTastingSession()
    {
        var sessionName = $"Other user add coffee session {Guid.NewGuid()}";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var session = new CuppingSession
        {
            Name = sessionName,
            Description = "Other user session",
            CreatedAt = DateTime.Now,
            UserId = 2
        };

        context.CuppingSessions.Add(session);
        await context.SaveChangesAsync();

        var request = new
        {
            coffeeId = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/TastingSessions/{session.Id}/coffees", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task EnsureTestUsersExistAsync(BrewDbContext context)
    {
        var firstUserExists = await context.AppUsers.AnyAsync(user => user.Id == 1);

        if (!firstUserExists)
        {
            context.AppUsers.Add(new AppUser
            {
                Id = 1,
                Email = "test-user@brewbase.local",
                Login = "test-user",
                PasswordHash = "test-hash",
                Role = "User",
                CreatedAt = DateTime.Now
            });
        }

        var secondUserExists = await context.AppUsers.AnyAsync(user => user.Id == 2);

        if (!secondUserExists)
        {
            context.AppUsers.Add(new AppUser
            {
                Id = 2,
                Email = "other-user@brewbase.local",
                Login = "other-user",
                PasswordHash = "test-hash",
                Role = "User",
                CreatedAt = DateTime.Now
            });
        }

        await context.SaveChangesAsync();
    }

	[Fact]
public async Task ShouldSaveNoteForCoffeeInTastingSession()
{
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    await EnsureTestUsersExistAsync(context);

    var session = new CuppingSession
    {
        Name = $"Note session {Guid.NewGuid()}",
        Description = "Session for note",
        CreatedAt = DateTime.Now,
        UserId = 1
    };

    context.CuppingSessions.Add(session);
    await context.SaveChangesAsync();

    context.CuppingSessionCoffees.Add(new CuppingSessionCoffee
    {
        CuppingSessionId = session.Id,
        CoffeeId = 1,
        CreatedAt = DateTime.Now
    });

    await context.SaveChangesAsync();

    var request = new
    {
        notes = "Bright acidity and floral aroma"
    };

    var response = await _client.PutAsJsonAsync(
        $"/api/TastingSessions/{session.Id}/coffees/1/note",
        request);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var payload = await response.Content.ReadAsStringAsync();
    using var document = JsonDocument.Parse(payload);
    var root = document.RootElement;

    Assert.Equal(1, root.GetProperty("coffeeId").GetInt32());
    Assert.Equal("Alpha Coffee", root.GetProperty("coffeeName").GetString());
    Assert.Equal("Bright acidity and floral aroma", root.GetProperty("notes").GetString());

    var savedSessionCoffee = await context.CuppingSessionCoffees
    	.AsNoTracking()
    	.SingleAsync(sessionCoffee =>
        	sessionCoffee.CuppingSessionId == session.Id &&
        	sessionCoffee.CoffeeId == 1);

    Assert.Equal("Bright acidity and floral aroma", savedSessionCoffee.Notes);
}

[Fact]
public async Task ShouldUpdateExistingCoffeeNoteInTastingSession()
{
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    await EnsureTestUsersExistAsync(context);

    var session = new CuppingSession
    {
        Name = $"Edit note session {Guid.NewGuid()}",
        Description = "Session for editing note",
        CreatedAt = DateTime.Now,
        UserId = 1
    };

    context.CuppingSessions.Add(session);
    await context.SaveChangesAsync();

    context.CuppingSessionCoffees.Add(new CuppingSessionCoffee
    {
        CuppingSessionId = session.Id,
        CoffeeId = 1,
        Notes = "Old note",
        CreatedAt = DateTime.Now
    });

    await context.SaveChangesAsync();

    var request = new
    {
        notes = "Updated note"
    };

    var response = await _client.PutAsJsonAsync(
        $"/api/TastingSessions/{session.Id}/coffees/1/note",
        request);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var savedSessionCoffee = await context.CuppingSessionCoffees
    	.AsNoTracking()
    	.SingleAsync(sessionCoffee =>
        	sessionCoffee.CuppingSessionId == session.Id &&
        	sessionCoffee.CoffeeId == 1);

    Assert.Equal("Updated note", savedSessionCoffee.Notes);
}

[Fact]
public async Task ShouldReturnNotFoundWhenSavingNoteForCoffeeOutsideTastingSession()
{
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    await EnsureTestUsersExistAsync(context);

    var session = new CuppingSession
    {
        Name = $"Outside coffee note session {Guid.NewGuid()}",
        Description = "Session without selected coffee",
        CreatedAt = DateTime.Now,
        UserId = 1
    };

    context.CuppingSessions.Add(session);
    await context.SaveChangesAsync();

    var request = new
    {
        notes = "This should fail"
    };

    var response = await _client.PutAsJsonAsync(
        $"/api/TastingSessions/{session.Id}/coffees/1/note",
        request);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}

[Fact]
public async Task ShouldReturnNotFoundWhenEditingNoteInOtherUserTastingSession()
{
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    await EnsureTestUsersExistAsync(context);

    var session = new CuppingSession
    {
        Name = $"Other user note session {Guid.NewGuid()}",
        Description = "Other user session",
        CreatedAt = DateTime.Now,
        UserId = 2
    };

    context.CuppingSessions.Add(session);
    await context.SaveChangesAsync();

    context.CuppingSessionCoffees.Add(new CuppingSessionCoffee
    {
        CuppingSessionId = session.Id,
        CoffeeId = 1,
        Notes = "Other user note",
        CreatedAt = DateTime.Now
    });

    await context.SaveChangesAsync();

    var request = new
    {
        notes = "Trying to edit"
    };

    var response = await _client.PutAsJsonAsync(
        $"/api/TastingSessions/{session.Id}/coffees/1/note",
        request);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
}