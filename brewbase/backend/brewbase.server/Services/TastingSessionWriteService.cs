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
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            SessionDate = request.SessionDate.HasValue
                ? DateTime.SpecifyKind(request.SessionDate.Value, DateTimeKind.Unspecified)
                : null,
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
            UserId = tastingSession.UserId,
            SessionDate = tastingSession.SessionDate
        };
    }

    public async Task<TastingSessionWriteResult<TastingSessionResponseDto>> UpdateSessionAsync(
        int sessionId,
        UpdateTastingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return new TastingSessionWriteResult<TastingSessionResponseDto>(
                TastingSessionWriteStatus.Unauthorized);
        }

        var session = await _context.CuppingSessions
            .SingleOrDefaultAsync(existingSession =>
                existingSession.Id == sessionId &&
                existingSession.UserId == userId.Value);

        if (session is null)
        {
            return new TastingSessionWriteResult<TastingSessionResponseDto>(
                TastingSessionWriteStatus.TastingSessionNotFound);
        }

        session.Name = request.Name.Trim();
        session.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        session.SessionDate = request.SessionDate.HasValue
            ? DateTime.SpecifyKind(request.SessionDate.Value, DateTimeKind.Unspecified)
            : null;

        await _context.SaveChangesAsync();

        return new TastingSessionWriteResult<TastingSessionResponseDto>(
            TastingSessionWriteStatus.Success,
            new TastingSessionResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                UserId = session.UserId,
                SessionDate = session.SessionDate
            });
    }

    public async Task<TastingSessionWriteStatus> DeleteSessionAsync(int sessionId)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return TastingSessionWriteStatus.Unauthorized;
        }

        var session = await _context.CuppingSessions
            .Include(existingSession => existingSession.CuppingSessionCoffees)
            .SingleOrDefaultAsync(existingSession =>
                existingSession.Id == sessionId &&
                existingSession.UserId == userId.Value);

        if (session is null)
        {
            return TastingSessionWriteStatus.TastingSessionNotFound;
        }

        _context.CuppingSessionCoffees.RemoveRange(session.CuppingSessionCoffees);
        _context.CuppingSessions.Remove(session);
        await _context.SaveChangesAsync();

        return TastingSessionWriteStatus.Success;
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

        var coffeeName = string.IsNullOrWhiteSpace(request.CoffeeName)
            ? null
            : request.CoffeeName.Trim();

        if (request.CoffeeId is null && coffeeName is null)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.InvalidCoffeeData);
        }

        Coffee? coffee = null;

        if (request.CoffeeId is not null)
        {
            coffee = await _context.Coffees
                .SingleOrDefaultAsync(coffee => coffee.Id == request.CoffeeId.Value);

            if (coffee is null)
            {
                return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                    TastingSessionWriteStatus.CoffeeNotFound);
            }

            var coffeeAlreadyAdded = await _context.CuppingSessionCoffees
                .AnyAsync(sessionCoffee =>
                    sessionCoffee.CuppingSessionId == sessionId &&
                    sessionCoffee.CoffeeId == request.CoffeeId.Value);

            if (coffeeAlreadyAdded)
            {
                return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                    TastingSessionWriteStatus.CoffeeAlreadyAdded);
            }
        }

        var sessionCoffee = new CuppingSessionCoffee
        {
            CuppingSessionId = sessionId,
            CoffeeId = request.CoffeeId,
            CustomCoffeeName = request.CoffeeId is null ? coffeeName : null,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _context.CuppingSessionCoffees.Add(sessionCoffee);
        await _context.SaveChangesAsync();

        var response = new TastingSessionCoffeeResponseDto
        {
            SessionCoffeeId = sessionCoffee.Id,
            CoffeeId = sessionCoffee.CoffeeId,
            CoffeeName = coffee?.Name ?? sessionCoffee.CustomCoffeeName!,
            Notes = sessionCoffee.Notes,
            AromaScore = sessionCoffee.AromaScore,
            SweetnessScore = sessionCoffee.SweetnessScore,
            AcidityScore = sessionCoffee.AcidityScore,
            BodyScore = sessionCoffee.BodyScore,
            FlavorProfileNotes = sessionCoffee.FlavorProfileNotes,
            CleanCup = sessionCoffee.CleanCup,
            OverallScore = sessionCoffee.OverallScore
        };

        return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            TastingSessionWriteStatus.Success,
            response);
    }
	
	public async Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeAsync(
    int sessionId,
    int sessionCoffeeId,
    UpdateTastingSessionCoffeeRequestDto request)
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
            .Include(sessionCoffee => sessionCoffee.Coffee)
            .SingleOrDefaultAsync(sessionCoffee =>
                sessionCoffee.CuppingSessionId == sessionId &&
                sessionCoffee.Id == sessionCoffeeId);

        if (sessionCoffee is null)
        {
            return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
                TastingSessionWriteStatus.CoffeeNotInSession);
        }

        sessionCoffee.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? null
            : request.Notes.Trim();

        sessionCoffee.AromaScore = request.AromaScore;
        sessionCoffee.SweetnessScore = request.SweetnessScore;
        sessionCoffee.AcidityScore = request.AcidityScore;
        sessionCoffee.BodyScore = request.BodyScore;
        sessionCoffee.FlavorProfileNotes = string.IsNullOrWhiteSpace(request.FlavorProfileNotes)
            ? null
            : request.FlavorProfileNotes.Trim();
        sessionCoffee.CleanCup = request.CleanCup;
        sessionCoffee.OverallScore = request.OverallScore;

        await _context.SaveChangesAsync();

        var response = new TastingSessionCoffeeResponseDto
        {
            SessionCoffeeId = sessionCoffee.Id,
            CoffeeId = sessionCoffee.CoffeeId,
            CoffeeName = sessionCoffee.Coffee?.Name ?? sessionCoffee.CustomCoffeeName!,
            Notes = sessionCoffee.Notes,
            AromaScore = sessionCoffee.AromaScore,
            SweetnessScore = sessionCoffee.SweetnessScore,
            AcidityScore = sessionCoffee.AcidityScore,
            BodyScore = sessionCoffee.BodyScore,
            FlavorProfileNotes = sessionCoffee.FlavorProfileNotes,
            CleanCup = sessionCoffee.CleanCup,
            OverallScore = sessionCoffee.OverallScore
        };

        return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            TastingSessionWriteStatus.Success,
            response);
    }
    
    public async Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
    int sessionId,
    int sessionCoffeeId,
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
        .Include(sessionCoffee => sessionCoffee.Coffee)
        .SingleOrDefaultAsync(sessionCoffee =>
            sessionCoffee.CuppingSessionId == sessionId &&
            sessionCoffee.Id == sessionCoffeeId);

    if (sessionCoffee is null)
    {
        return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
            TastingSessionWriteStatus.CoffeeNotInSession);
    }

    sessionCoffee.Notes = string.IsNullOrWhiteSpace(request.Notes)
        ? null
        : request.Notes.Trim();

    await _context.SaveChangesAsync();

    var response = new TastingSessionCoffeeResponseDto
    {
        SessionCoffeeId = sessionCoffee.Id,
        CoffeeId = sessionCoffee.CoffeeId,
        CoffeeName = sessionCoffee.Coffee?.Name ?? sessionCoffee.CustomCoffeeName!,
        Notes = sessionCoffee.Notes,
        AromaScore = sessionCoffee.AromaScore,
        SweetnessScore = sessionCoffee.SweetnessScore,
        AcidityScore = sessionCoffee.AcidityScore,
        BodyScore = sessionCoffee.BodyScore,
        FlavorProfileNotes = sessionCoffee.FlavorProfileNotes,
        CleanCup = sessionCoffee.CleanCup,
        OverallScore = sessionCoffee.OverallScore
    };

    return new TastingSessionWriteResult<TastingSessionCoffeeResponseDto>(
        TastingSessionWriteStatus.Success,
        response);
}

    public async Task<TastingSessionWriteStatus> DeleteCoffeeAsync(int sessionId, int sessionCoffeeId)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return TastingSessionWriteStatus.Unauthorized;
        }

        var sessionExists = await _context.CuppingSessions
            .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

        if (!sessionExists)
        {
            return TastingSessionWriteStatus.TastingSessionNotFound;
        }

        var sessionCoffee = await _context.CuppingSessionCoffees
            .SingleOrDefaultAsync(existingCoffee =>
                existingCoffee.CuppingSessionId == sessionId &&
                existingCoffee.Id == sessionCoffeeId);

        if (sessionCoffee is null)
        {
            return TastingSessionWriteStatus.CoffeeNotInSession;
        }

        _context.CuppingSessionCoffees.Remove(sessionCoffee);
        await _context.SaveChangesAsync();

        return TastingSessionWriteStatus.Success;
    }
}