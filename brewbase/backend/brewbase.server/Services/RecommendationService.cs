using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Dtos;

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

        var recommendations = await LoadRecommendationsForUserAsync(userId.Value);

        if (recommendations.Count == 0)
        {
            await RefreshRecommendationsForUserAsync(userId.Value);
            recommendations = await LoadRecommendationsForUserAsync(userId.Value);
        }

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
                        MatchScore = recommendation.MatchScore,
                        PopularityScore = recommendation.PopularityScore,
                        FinalScore = recommendation.FinalScore
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
                        MatchScore = recommendation.MatchScore,
                        PopularityScore = recommendation.PopularityScore,
                        FinalScore = recommendation.FinalScore
                    };
                })
                .Take(10)
                .ToList()
        };
    }
    
    private async Task<List<Recommendation>> LoadRecommendationsForUserAsync(int userId)
    {
        return await _context.Recommendations
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
                recommendation.UserId == userId &&
                recommendation.Algorithm == "cron-recommendation-v1")
            .OrderByDescending(recommendation => recommendation.FinalScore)
            .ThenByDescending(recommendation => recommendation.GeneratedAt)
            .ToListAsync();
    }
    
    private async Task RefreshRecommendationsForUserAsync(int userId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "SELECT refresh_all_rankings(); SELECT refresh_recommendations_for_user({0});",
            userId);
    }

	public async Task SubmitSummaryFeedbackAsync(RecommendationSummaryFeedbackRequestDto request)
{
    var userId = _currentUserProvider.GetUserId();

    if (userId == null)
    {
        throw new Exception("User not found");
    }

    if (request.Rating < 1 || request.Rating > 5)
    {
        throw new ArgumentException("Rating must be between 1 and 5");
    }

    var preference = await _context.UserPreferences
        .FirstOrDefaultAsync(item => item.UserId == userId.Value);

    if (preference == null)
    {
        throw new KeyNotFoundException("User preferences not found");
    }

    var previousStyle = preference.RecommendationStyle;

    var normalizedAction = request.PreferenceAction.Trim().ToLowerInvariant();

    var newStyle = normalizedAction switch
    {
        "more_similar" => "safe",
        "more_diverse" => "explore",
        "no_change" => request.Rating <= 2 ? "explore" : request.Rating >= 4 ? "safe" : "balanced",
        _ => request.Rating <= 2 ? "explore" : request.Rating >= 4 ? "safe" : "balanced"
    };


    preference.RecommendationStyle = newStyle;

    _context.RecommendationFeedbackSummaries.Add(new RecommendationFeedbackSummary
    {
        UserId = userId.Value,
        Rating = request.Rating,
        PreferenceAction = normalizedAction,
        PreviousRecommendationStyle = previousStyle,
        NewRecommendationStyle = newStyle,
		CreatedAt = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();

    await _context.Database.ExecuteSqlRawAsync(
        "SELECT refresh_all_rankings(); SELECT refresh_recommendations_for_user({0});",
        userId.Value);
}
    
}