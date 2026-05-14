using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RankingReadService : IRankingReadService
{
    private readonly BrewDbContext _context;

    public RankingReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoffeeRankingResponseDto>> GetCoffeeRankingAsync(int limit)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);

        var ranking = await _context.Coffees
            .AsNoTracking()
            .Select(coffee => new CoffeeRankingResponseDto
            {
                CoffeeId = coffee.Id,
                Name = coffee.Name,
                Region = coffee.Region.Name,
                Roastery = coffee.Roastery.Name,
                ProcessingMethod = coffee.ProcessingMethod != null
                    ? coffee.ProcessingMethod.Name
                    : null,
                Variety = coffee.Variety != null
                    ? coffee.Variety.Name
                    : null,
                AverageRating = coffee.CoffeeRatings
                    .Average(rating => (double?)rating.Value) ?? 0,
                RatingCount = coffee.CoffeeRatings.Count,
                RecipeUsedCount = coffee.Recipes.Count
            })
            .Where(coffee => coffee.RatingCount > 0)
            .OrderByDescending(coffee => coffee.AverageRating)
            .ThenByDescending(coffee => coffee.RatingCount)
            .ThenByDescending(coffee => coffee.RecipeUsedCount)
            .ThenBy(coffee => coffee.Name)
            .Take(safeLimit)
            .ToListAsync();

        for (var index = 0; index < ranking.Count; index++)
        {
            ranking[index].Position = index + 1;
        }

        return ranking;
    }
}