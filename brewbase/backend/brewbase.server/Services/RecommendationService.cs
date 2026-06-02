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
            throw new Exception("User not found");

        var preference = await _context.UserPreferences
            .Include(x => x.UserPreferenceRegions)
            .Include(x => x.UserPreferenceBrewingMethods)
            .Include(x => x.UserPreferenceFlavorProfiles)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (preference == null || !preference.QuizCompleted)
        {
            return new RecommendationResponseDto
            {
                Coffees = await GetFallbackCoffeesAsync(),
                Recipes = await GetFallbackRecipesAsync()
            };
        }

        return new RecommendationResponseDto
        {
            Coffees = await GenerateCoffeeRecommendationsAsync(preference),
            Recipes = await GenerateRecipeRecommendationsAsync(preference)
        };
    }

    private async Task<List<CoffeeRecommendationDto>> GetFallbackCoffeesAsync()
    {
        return await _context.CoffeeRankings
            .Include(x => x.Coffee)
            .OrderByDescending(x => x.RankingScore)
            .Take(10)
            .Select(x => new CoffeeRecommendationDto
            {
                CoffeeId = x.CoffeeId,
                Name = x.Coffee.Name,
                MatchScore = 0,
                PopularityScore = x.RankingScore,
                FinalScore = x.RankingScore
            })
            .ToListAsync();
    }

    private async Task<List<RecipeRecommendationDto>> GetFallbackRecipesAsync()
    {
        return await _context.RecipeRankings
            .Include(x => x.Recipe)
            .OrderByDescending(x => x.RankingScore)
            .Take(10)
            .Select(x => new RecipeRecommendationDto
            {
                RecipeId = x.RecipeId,
                Title = x.Recipe.Title,
                MatchScore = 0,
                PopularityScore = x.RankingScore,
                FinalScore = x.RankingScore
            })
            .ToListAsync();
    }
    
    private async Task<List<CoffeeRecommendationDto>> GenerateCoffeeRecommendationsAsync(UserPreference preference)
    {
        var preferredRegions =
            preference.UserPreferenceRegions
                .Select(x => x.RegionId)
                .ToHashSet();

        var coffees = await _context.Coffees
            .Include(x => x.CoffeeRankings)
            .Include(x => x.Body)
            .Include(x => x.Acidity)
            .ToListAsync();

        var result =
            new List<CoffeeRecommendationDto>();

        foreach (var coffee in coffees)
        {
            double matchScore = 0;

            if (preferredRegions.Contains(coffee.RegionId))
            {
                matchScore += 20;
            }
            
            if (!string.IsNullOrWhiteSpace(preference.PreferredBody)
                && coffee.Body != null
                && coffee.Body.Name == preference.PreferredBody)
            {
                matchScore += 15;
            }
            
            if (!string.IsNullOrWhiteSpace(preference.PreferredAcidity)
                && coffee.Acidity != null
                && coffee.Acidity.Name == preference.PreferredAcidity)
            {
                matchScore += 15;
            }

            var ranking = coffee.CoffeeRankings
                .OrderByDescending(x => x.RefreshedAt)
                .FirstOrDefault();

            var popularityScore =
                ranking?.RankingScore ?? 0;

            double matchWeight = 0.5;
            double popularityWeight = 0.5;

            switch (preference.RecommendationStyle)
            {
                case "Bezpieczne wybory":
                    matchWeight = 0.8;
                    popularityWeight = 0.2;
                    break;

                case "Zbalansowane":
                    matchWeight = 0.5;
                    popularityWeight = 0.5;
                    break;

                case "Zaskocz mnie":
                    matchWeight = 0.2;
                    popularityWeight = 0.8;
                    break;
            }

            var finalScore =
                (matchScore * matchWeight)
                + (popularityScore * popularityWeight);

            result.Add(
                new CoffeeRecommendationDto
                {
                    CoffeeId = coffee.Id,
                    Name = coffee.Name,
                    MatchScore = matchScore,
                    PopularityScore = popularityScore,
                    FinalScore = finalScore
                });
        }
        
        if (!preference.AllowExploration)
        {
            result = result
                .Where(x => x.MatchScore > 0)
                .ToList();
        }

        return result
            .OrderByDescending(x => x.FinalScore)
            .Take(10)
            .ToList();
    }
    
    private async Task<List<RecipeRecommendationDto>> GenerateRecipeRecommendationsAsync(UserPreference preference)
    {
        var preferredRegions =
            preference.UserPreferenceRegions
                .Select(x => x.RegionId)
                .ToHashSet();

        var preferredMethods =
            preference.UserPreferenceBrewingMethods
                .Select(x => x.BrewingMethodId)
                .ToHashSet();

        var recipes = await _context.Recipes
            .Include(x => x.Coffee)
            .Include(x => x.RecipeRankings)
            .Where(x => x.IsPublic)
            .ToListAsync();

        var result =
            new List<RecipeRecommendationDto>();

        foreach (var recipe in recipes)
        {
            double matchScore = 0;

            if (recipe.BrewingMethodId.HasValue &&
                preferredMethods.Contains(
                    recipe.BrewingMethodId.Value))
            {
                matchScore += 20;
            }

            if (recipe.Coffee != null &&
                preferredRegions.Contains(
                    recipe.Coffee.RegionId))
            {
                matchScore += 15;
            }

            var ranking = recipe.RecipeRankings
                .OrderByDescending(x => x.RefreshedAt)
                .FirstOrDefault();

            var popularityScore =
                ranking?.RankingScore ?? 0;

            double matchWeight = 0.5;
            double popularityWeight = 0.5;

            switch (preference.RecommendationStyle)
            {
                case "Bezpieczne wybory":
                    matchWeight = 0.8;
                    popularityWeight = 0.2;
                    break;

                case "Zbalansowane":
                    matchWeight = 0.5;
                    popularityWeight = 0.5;
                    break;

                case "Zaskocz mnie":
                    matchWeight = 0.2;
                    popularityWeight = 0.8;
                    break;
            }

            var finalScore =
                (matchScore * matchWeight)
                + (popularityScore * popularityWeight);

            result.Add(
                new RecipeRecommendationDto
                {
                    RecipeId = recipe.Id,
                    Title = recipe.Title,
                    MatchScore = matchScore,
                    PopularityScore = popularityScore,
                    FinalScore = finalScore
                });
        }
        
        if (!preference.AllowExploration)
        {
            result = result
                .Where(x => x.MatchScore > 0)
                .ToList();
        }

        return result
            .OrderByDescending(x => x.FinalScore)
            .Take(10)
            .ToList();
    }
}