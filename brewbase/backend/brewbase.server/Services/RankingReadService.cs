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

        return await _context.CoffeeRankings
            .AsNoTracking()
            .Where(ranking => ranking.Position > 0)
            .OrderBy(ranking => ranking.Position)
            .Take(safeLimit)
            .Select(ranking => new CoffeeRankingResponseDto
            {
                Position = ranking.Position,
                CoffeeId = ranking.CoffeeId,
                Name = ranking.Coffee.Name,
                Region = ranking.Coffee.Region != null
                    ? ranking.Coffee.Region.Name
                    : null,
                Roastery = ranking.Coffee.Roastery != null
                    ? ranking.Coffee.Roastery.Name
                    : null,
                ProcessingMethod = ranking.Coffee.ProcessingMethod != null
                    ? ranking.Coffee.ProcessingMethod.Name
                    : null,
                Variety = ranking.Coffee.Variety != null
                    ? ranking.Coffee.Variety.Name
                    : null,
                AverageRating = ranking.AverageRating,
                RatingCount = ranking.RatingCount,
                RecipeUsedCount = ranking.RecipeUsedCount
            })
            .ToListAsync();
    }
    
    public async Task<List<UserRankingResponseDto>> GetUserRankingAsync(int limit)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);

        return await _context.UserRankings
            .AsNoTracking()
            .Where(ranking => ranking.Position > 0)
            .OrderBy(ranking => ranking.Position)
            .Take(safeLimit)
            .Select(ranking => new UserRankingResponseDto
            {
                Position = ranking.Position,
                UserId = ranking.UserId,
                Login = ranking.User.Login,
                ActivityScore = ranking.ActivityScore,
                PublicRecipeCount = ranking.PublicRecipeCount,
                CoffeeRatingCount = ranking.CoffeeRatingCount,
                RecipeRatingCount = ranking.RecipeRatingCount,
                QuickNoteCount = ranking.QuickNoteCount,
                CuppingSessionCount = ranking.CuppingSessionCount,
                CuppingSessionCoffeeCount = ranking.CuppingSessionCoffeeCount,
                FollowersCount = ranking.FollowersCount,
                ReceivedRecipeFavoriteCount = ranking.ReceivedRecipeFavoriteCount,
                PublishedArticleCount = ranking.PublishedArticleCount
            })
            .ToListAsync();
    }
    
    public async Task<List<RecipeRankingResponseDto>> GetRecipeRankingAsync(int limit)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);

        return await _context.RecipeRankings
            .AsNoTracking()
            .Where(ranking => ranking.Position > 0)
            .OrderBy(ranking => ranking.Position)
            .Take(safeLimit)
            .Select(ranking => new RecipeRankingResponseDto
            {
                Position = ranking.Position,
                RecipeId = ranking.RecipeId,
                Title = ranking.Recipe.Title,
                Coffee = ranking.Recipe.Coffee != null
                    ? ranking.Recipe.Coffee.Name
                    : null,
                BrewingMethod = ranking.Recipe.BrewingMethod != null
                    ? ranking.Recipe.BrewingMethod.Name
                    : null,
                UserId = ranking.Recipe.UserId,
                UserLogin = ranking.Recipe.User.Login,
                AverageRating = ranking.AverageRating,
                RatingCount = ranking.RatingCount,
                SaveCount = ranking.SaveCount
            })
            .ToListAsync();
    }
    
}