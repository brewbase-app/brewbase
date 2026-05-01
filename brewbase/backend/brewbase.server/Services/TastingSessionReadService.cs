using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class TastingSessionReadService : ITastingSessionReadService
{
    private readonly BrewDbContext _context;

    public TastingSessionReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<TastingSessionListItemResponseDto>> GetUserSessionsAsync(int userId)
    {
        return await _context.CuppingSessions
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.CreatedAt)
            .Select(session => new TastingSessionListItemResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                CoffeeCount = session.CuppingSessionCoffees.Count
            })
            .ToListAsync();
    }

    public async Task<TastingSessionDetailsResponseDto?> GetSessionDetailsAsync(int id, int userId)
    {
        return await _context.CuppingSessions
            .Where(session => session.Id == id && session.UserId == userId)
            .Select(session => new TastingSessionDetailsResponseDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                CreatedAt = session.CreatedAt,
                Coffees = session.CuppingSessionCoffees
                    .Select(sessionCoffee => new TastingSessionCoffeeResponseDto
                    {
                        CoffeeId = sessionCoffee.CoffeeId,
                        CoffeeName = sessionCoffee.Coffee.Name,
                        Notes = sessionCoffee.Notes
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }
}