using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class TastingSessionWriteService : ITastingSessionWriteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TastingSessionWriteService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<TastingSessionResponseDto?> CreateAsync(CreateTastingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return null;
        }

        var tastingSession = new CuppingSession
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            UserId = userId.Value
        };

        _context.CuppingSessions.Add(tastingSession);
        await _context.SaveChangesAsync();

        return new TastingSessionResponseDto
        {
            Id = tastingSession.Id,
            Name = tastingSession.Name,
            Description = tastingSession.Description,
            CreatedAt = tastingSession.CreatedAt,
            UserId = tastingSession.UserId
        };
    }
	
	public async Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> AddCoffeeAsync(
        int sessionId,
        AddCoffeeToTastingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.Unauthorized);
        }

        var sessionExists = await _context.CuppingSessions
            .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

        if (!sessionExists)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.TastingSessionNotFound);
        }

        var coffee = await _context.Coffees
            .Where(coffee => coffee.Id == request.CoffeeId)
            .Select(coffee => new
            {
                coffee.Id,
                coffee.Name
            })
            .SingleOrDefaultAsync();

        if (coffee is null)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.CoffeeNotFound);
        }

        var coffeeAlreadyAdded = await _context.CuppingSessionCoffees
            .AnyAsync(sessionCoffee =>
                sessionCoffee.CuppingSessionId == sessionId &&
                sessionCoffee.CoffeeId == request.CoffeeId);

        if (coffeeAlreadyAdded)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.CoffeeAlreadyAdded);
        }

        var sessionCoffee = new CuppingSessionCoffee
        {
            CuppingSessionId = sessionId,
            CoffeeId = request.CoffeeId,
			Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _context.CuppingSessionCoffees.Add(sessionCoffee);
        await _context.SaveChangesAsync();

        var response = new TastingSessionCoffeeResponseDto
        {
            CoffeeId = coffee.Id,
            CoffeeName = coffee.Name,
            Notes = sessionCoffee.Notes
        };

        return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            TastingSessionWriteStatus.Success,
            response);
    }
	
	public async Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
    int sessionId,
    int coffeeId,
    UpdateTastingSessionCoffeeNoteRequestDto request)
	{	
    	var userId = _currentUserProvider.GetUserId();

    	if (userId is null)
    	{
        	return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            	TastingSessionWriteStatus.Unauthorized);
    	}

    	var sessionExists = await _context.CuppingSessions
        	.AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

    	if (!sessionExists)
    	{
        	return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            	TastingSessionWriteStatus.TastingSessionNotFound);
    	}

    	var sessionCoffee = await _context.CuppingSessionCoffees
        	.SingleOrDefaultAsync(sessionCoffee =>
            	sessionCoffee.CuppingSessionId == sessionId &&
            	sessionCoffee.CoffeeId == coffeeId);

    	if (sessionCoffee is null)
    	{
        	return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            	TastingSessionWriteStatus.CoffeeNotInSession);
    	}

    	var coffee = await _context.Coffees
        	.Where(coffee => coffee.Id == coffeeId)
        	.Select(coffee => new
        	{
            	coffee.Id,
            	coffee.Name
        	})
        	.SingleAsync();

    	sessionCoffee.Notes = string.IsNullOrWhiteSpace(request.Notes)
    		? null
    		: request.Notes.Trim();

    	await _context.SaveChangesAsync();

    	var response = new TastingSessionCoffeeResponseDto
    	{
        	CoffeeId = coffee.Id,
        	CoffeeName = coffee.Name,
        	Notes = sessionCoffee.Notes
    	};

    	return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
        	TastingSessionWriteStatus.Success,
        	response);
	}
}