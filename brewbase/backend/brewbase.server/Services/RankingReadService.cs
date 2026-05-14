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
    
    public async Task<List<UserRankingResponseDto>> GetUserRankingAsync(int limit)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);

        var ranking = await _context.AppUsers
            .AsNoTracking()
            .Where(user => !user.IsBlocked)
            .Select(user => new UserRankingResponseDto
            {
                UserId = user.Id,
                Login = user.Login,

                PublicRecipeCount = user.Recipes.Count(recipe => recipe.IsPublic),
                CoffeeRatingCount = user.CoffeeRatings.Count,
                RecipeRatingCount = user.RecipeRatings.Count,
                QuickNoteCount = user.QuickNotes.Count,
                CuppingSessionCount = user.CuppingSessions.Count,
                CuppingSessionCoffeeCount = user.CuppingSessions
                    .SelectMany(session => session.CuppingSessionCoffees)
                    .Count(),
                FollowersCount = user.FollowFolloweds.Count,
                ReceivedRecipeFavoriteCount = user.Recipes
                    .SelectMany(recipe => recipe.UserRecipeFavorites)
                    .Count(),
                PublishedArticleCount = user.ArticleUsers
                    .Count(article => article.Status == "APPROVED")
            })
            .ToListAsync();

        foreach (var user in ranking)
        {
            user.ActivityScore =
                user.PublicRecipeCount * 10
                + user.CoffeeRatingCount * 3
                + user.RecipeRatingCount * 3
                + user.QuickNoteCount * 2
                + user.CuppingSessionCount * 8
                + user.CuppingSessionCoffeeCount * 2
                + user.FollowersCount * 5
                + user.ReceivedRecipeFavoriteCount * 4
                + user.PublishedArticleCount * 12;
        }

        ranking = ranking
            .Where(user => user.ActivityScore > 0)
            .OrderByDescending(user => user.ActivityScore)
            .ThenByDescending(user => user.PublicRecipeCount)
            .ThenByDescending(user => user.FollowersCount)
            .ThenBy(user => user.Login)
            .Take(safeLimit)
            .ToList();

        for (var index = 0; index < ranking.Count; index++)
        {
            ranking[index].Position = index + 1;
        }

        return ranking;
    }
    
}