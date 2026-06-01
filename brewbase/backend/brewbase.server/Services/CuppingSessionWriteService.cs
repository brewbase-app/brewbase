using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class CuppingSessionWriteService : ICuppingSessionWriteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CuppingSessionWriteService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<CuppingSessionResponseDto?> CreateAsync(CreateCuppingSessionRequestDto request)
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

        return new CuppingSessionResponseDto
        {
            Id = tastingSession.Id,
            Name = tastingSession.Name,
            Description = tastingSession.Description,
            CreatedAt = tastingSession.CreatedAt,
            UserId = tastingSession.UserId,
            SessionDate = tastingSession.SessionDate
        };
    }

    public async Task<CuppingSessionWriteResult<CuppingSessionResponseDto>> UpdateSessionAsync(
        int sessionId,
        UpdateCuppingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionResponseDto>(
                CuppingSessionWriteStatus.Unauthorized);
        }

        var session = await _context.CuppingSessions
            .SingleOrDefaultAsync(existingSession =>
                existingSession.Id == sessionId &&
                existingSession.UserId == userId.Value);

        if (session is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionResponseDto>(
                CuppingSessionWriteStatus.CuppingSessionNotFound);
        }

        session.Name = request.Name.Trim();
        session.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        session.SessionDate = request.SessionDate.HasValue
            ? DateTime.SpecifyKind(request.SessionDate.Value, DateTimeKind.Unspecified)
            : null;

        await _context.SaveChangesAsync();

        return new CuppingSessionWriteResult<CuppingSessionResponseDto>(
            CuppingSessionWriteStatus.Success,
            new CuppingSessionResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                UserId = session.UserId,
                SessionDate = session.SessionDate
            });
    }

    public async Task<CuppingSessionWriteStatus> DeleteSessionAsync(int sessionId)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return CuppingSessionWriteStatus.Unauthorized;
        }

        var session = await _context.CuppingSessions
            .Include(existingSession => existingSession.CuppingSessionCoffees)
            .SingleOrDefaultAsync(existingSession =>
                existingSession.Id == sessionId &&
                existingSession.UserId == userId.Value);

        if (session is null)
        {
            return CuppingSessionWriteStatus.CuppingSessionNotFound;
        }

        _context.CuppingSessionCoffees.RemoveRange(session.CuppingSessionCoffees);
        _context.CuppingSessions.Remove(session);
        await _context.SaveChangesAsync();

        return CuppingSessionWriteStatus.Success;
    }
	
	public async Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> AddCoffeeAsync(
    int sessionId,
    AddCoffeeToCuppingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.Unauthorized);
        }

        var sessionExists = await _context.CuppingSessions
            .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

        if (!sessionExists)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.CuppingSessionNotFound);
        }

        var coffeeName = string.IsNullOrWhiteSpace(request.CoffeeName)
            ? null
            : request.CoffeeName.Trim();

        if (request.CoffeeId is null && coffeeName is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.InvalidCoffeeData);
        }

        Coffee? coffee = null;

        if (request.CoffeeId is not null)
        {
            coffee = await _context.Coffees
                .SingleOrDefaultAsync(coffee => coffee.Id == request.CoffeeId.Value);

            if (coffee is null)
            {
                return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                    CuppingSessionWriteStatus.CoffeeNotFound);
            }

            var coffeeAlreadyAdded = await _context.CuppingSessionCoffees
                .AnyAsync(sessionCoffee =>
                    sessionCoffee.CuppingSessionId == sessionId &&
                    sessionCoffee.CoffeeId == request.CoffeeId.Value);

            if (coffeeAlreadyAdded)
            {
                return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                    CuppingSessionWriteStatus.CoffeeAlreadyAdded);
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

        var response = new CuppingSessionCoffeeResponseDto
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

        return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
            CuppingSessionWriteStatus.Success,
            response);
    }
	
	public async Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> UpdateCoffeeAsync(
    int sessionId,
    int sessionCoffeeId,
    UpdateCuppingSessionCoffeeRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.Unauthorized);
        }

        var sessionExists = await _context.CuppingSessions
            .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

        if (!sessionExists)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.CuppingSessionNotFound);
        }

        var sessionCoffee = await _context.CuppingSessionCoffees
            .Include(sessionCoffee => sessionCoffee.Coffee)
            .SingleOrDefaultAsync(sessionCoffee =>
                sessionCoffee.CuppingSessionId == sessionId &&
                sessionCoffee.Id == sessionCoffeeId);

        if (sessionCoffee is null)
        {
            return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
                CuppingSessionWriteStatus.CoffeeNotInSession);
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

        var response = new CuppingSessionCoffeeResponseDto
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

        return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
            CuppingSessionWriteStatus.Success,
            response);
    }
    
    public async Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
    int sessionId,
    int sessionCoffeeId,
    UpdateCuppingSessionCoffeeNoteRequestDto request)
{
    var userId = _currentUserProvider.GetUserId();

    if (userId is null)
    {
        return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
            CuppingSessionWriteStatus.Unauthorized);
    }

    var sessionExists = await _context.CuppingSessions
        .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

    if (!sessionExists)
    {
        return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
            CuppingSessionWriteStatus.CuppingSessionNotFound);
    }

    var sessionCoffee = await _context.CuppingSessionCoffees
        .Include(sessionCoffee => sessionCoffee.Coffee)
        .SingleOrDefaultAsync(sessionCoffee =>
            sessionCoffee.CuppingSessionId == sessionId &&
            sessionCoffee.Id == sessionCoffeeId);

    if (sessionCoffee is null)
    {
        return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
            CuppingSessionWriteStatus.CoffeeNotInSession);
    }

    sessionCoffee.Notes = string.IsNullOrWhiteSpace(request.Notes)
        ? null
        : request.Notes.Trim();

    await _context.SaveChangesAsync();

    var response = new CuppingSessionCoffeeResponseDto
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

    return new CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>(
        CuppingSessionWriteStatus.Success,
        response);
}

    public async Task<CuppingSessionWriteStatus> DeleteCoffeeAsync(int sessionId, int sessionCoffeeId)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return CuppingSessionWriteStatus.Unauthorized;
        }

        var sessionExists = await _context.CuppingSessions
            .AnyAsync(session => session.Id == sessionId && session.UserId == userId.Value);

        if (!sessionExists)
        {
            return CuppingSessionWriteStatus.CuppingSessionNotFound;
        }

        var sessionCoffee = await _context.CuppingSessionCoffees
            .SingleOrDefaultAsync(existingCoffee =>
                existingCoffee.CuppingSessionId == sessionId &&
                existingCoffee.Id == sessionCoffeeId);

        if (sessionCoffee is null)
        {
            return CuppingSessionWriteStatus.CoffeeNotInSession;
        }

        _context.CuppingSessionCoffees.Remove(sessionCoffee);
        await _context.SaveChangesAsync();

        return CuppingSessionWriteStatus.Success;
    }
}