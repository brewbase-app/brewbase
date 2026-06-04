using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CuppingSessionReadService : ICuppingSessionReadService
{
    private readonly BrewDbContext _context;

    public CuppingSessionReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CuppingSessionListItemResponseDto>> GetUserSessionsAsync(int userId)
    {
        return await _context.CuppingSessions
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.CreatedAt)
            .Select(session => new CuppingSessionListItemResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                SessionDate = session.SessionDate,
                CoffeeCount = session.CuppingSessionCoffees.Count
            })
            .ToListAsync();
    }

    public async Task<CuppingSessionDetailsResponseDto?> GetSessionDetailsAsync(int id, int userId)
    {
        return await _context.CuppingSessions
            .Where(session => session.Id == id && session.UserId == userId)
            .Select(session => new CuppingSessionDetailsResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                SessionDate = session.SessionDate,
                Coffees = session.CuppingSessionCoffees
                    .Select(sessionCoffee => new CuppingSessionCoffeeResponseDto
                    {
						SessionCoffeeId = sessionCoffee.Id,
						CoffeeId = sessionCoffee.CoffeeId,
						CoffeeName = sessionCoffee.Coffee != null
    						? sessionCoffee.Coffee.Name
    						: sessionCoffee.CustomCoffeeName!,
						Notes = sessionCoffee.Notes,
						AromaScore = sessionCoffee.AromaScore,
						SweetnessScore = sessionCoffee.SweetnessScore,
						AcidityScore = sessionCoffee.AcidityScore,
						BodyScore = sessionCoffee.BodyScore,
						FlavorProfileNotes = sessionCoffee.FlavorProfileNotes,
						OverallScore = sessionCoffee.OverallScore
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }
}