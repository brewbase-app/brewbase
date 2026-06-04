using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RecommendationService : IRecommendationService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RecommendationService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<RecommendationResponseDto> GetRecommendationsAsync()
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
        {
            throw new Exception("User not found");
        }

        var recommendations = await _context.Recommendations
            .AsNoTracking()
            .Include(recommendation => recommendation.Coffee)
                .ThenInclude(coffee => coffee!.Region)
            .Include(recommendation => recommendation.Coffee)
                .ThenInclude(coffee => coffee!.ProcessingMethod)
            .Include(recommendation => recommendation.Coffee)
                .ThenInclude(coffee => coffee!.Variety)
            .Include(recommendation => recommendation.Coffee)
                .ThenInclude(coffee => coffee!.Roastery)
            .Include(recommendation => recommendation.Recipe)
                .ThenInclude(recipe => recipe!.User)
            .Where(recommendation =>
                recommendation.UserId == userId.Value &&
                recommendation.Algorithm == "cron-recommendation-v1")
            .OrderByDescending(recommendation => recommendation.Score)
            .ThenByDescending(recommendation => recommendation.GeneratedAt)
            .ToListAsync();

        var coffeeIds = recommendations
            .Where(recommendation => recommendation.CoffeeId != null)
            .Select(recommendation => recommendation.CoffeeId!.Value)
            .Distinct()
            .ToList();

        var recipeIds = recommendations
            .Where(recommendation => recommendation.RecipeId != null)
            .Select(recommendation => recommendation.RecipeId!.Value)
            .Distinct()
            .ToList();

        var coffeeRankings = await _context.CoffeeRankings
            .AsNoTracking()
            .Where(ranking => coffeeIds.Contains(ranking.CoffeeId))
            .OrderByDescending(ranking => ranking.RefreshedAt)
            .ToListAsync();

        var recipeRankings = await _context.RecipeRankings
            .AsNoTracking()
            .Where(ranking => recipeIds.Contains(ranking.RecipeId))
            .OrderByDescending(ranking => ranking.RefreshedAt)
            .ToListAsync();

        var coffeeRankingByCoffeeId = coffeeRankings
            .GroupBy(ranking => ranking.CoffeeId)
            .ToDictionary(group => group.Key, group => group.First());

        var recipeRankingByRecipeId = recipeRankings
            .GroupBy(ranking => ranking.RecipeId)
            .ToDictionary(group => group.Key, group => group.First());

        return new RecommendationResponseDto
        {
            Coffees = recommendations
                .Where(recommendation =>
                    recommendation.CoffeeId != null &&
                    recommendation.Coffee != null)
                .Select(recommendation =>
                {
                    coffeeRankingByCoffeeId.TryGetValue(
                        recommendation.CoffeeId!.Value,
                        out var ranking);

                    return new CoffeeRecommendationDto
                    {
                        CoffeeId = recommendation.CoffeeId.Value,
                        Name = recommendation.Coffee!.Name,
                        Region = recommendation.Coffee.Region.Name,
                        ProcessingMethod = recommendation.Coffee.ProcessingMethod?.Name,
                        Variety = recommendation.Coffee.Variety?.Name,
                        Roastery = recommendation.Coffee.Roastery.Name,
                        AverageRating = ranking?.AverageRating ?? 0,
                        RatingCount = ranking?.RatingCount ?? 0,
                        MatchScore = recommendation.Score,
                        PopularityScore = ranking?.RankingScore ?? recommendation.Score,
                        FinalScore = recommendation.Score
                    };
                })
                .Take(10)
                .ToList(),

            Recipes = recommendations
                .Where(recommendation =>
                    recommendation.RecipeId != null &&
                    recommendation.Recipe != null)
                .Select(recommendation =>
                {
                    recipeRankingByRecipeId.TryGetValue(
                        recommendation.RecipeId!.Value,
                        out var ranking);

                    return new RecipeRecommendationDto
                    {
                        RecipeId = recommendation.RecipeId.Value,
                        Title = recommendation.Recipe!.Title,
                        UserLogin = recommendation.Recipe.User.Login,
                        AverageRating = ranking?.AverageRating ?? 0,
                        RatingCount = ranking?.RatingCount ?? 0,
                        MatchScore = recommendation.Score,
                        PopularityScore = ranking?.RankingScore ?? recommendation.Score,
                        FinalScore = recommendation.Score
                    };
                })
                .Take(10)
                .ToList()
        };
    }
}