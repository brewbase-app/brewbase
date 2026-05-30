using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RecipeFavoriteService : IRecipeFavoriteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RecipeFavoriteService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<FavoriteServiceStatus> AddAsync(int recipeId)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return FavoriteServiceStatus.Unauthorized;
        }

        var recipeExists = await RecipeReadService
            .WhereVisibleTo(_context.Recipes.AsNoTracking(), userId.Value)
            .AnyAsync(r => r.Id == recipeId);

        if (!recipeExists)
        {
            return FavoriteServiceStatus.NotFound;
        }

        var alreadyFavorite = await _context.UserRecipeFavorites
            .AnyAsync(f => f.UserId == userId.Value && f.RecipeId == recipeId);

        if (!alreadyFavorite)
        {
            _context.UserRecipeFavorites.Add(new UserRecipeFavorite
            {
                UserId = userId.Value,
                RecipeId = recipeId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });

            await _context.SaveChangesAsync();
        }

        return FavoriteServiceStatus.Success;
    }

    public async Task<FavoriteServiceStatus> RemoveAsync(int recipeId)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return FavoriteServiceStatus.Unauthorized;
        }

        var favorite = await _context.UserRecipeFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.RecipeId == recipeId);

        if (favorite is not null)
        {
            _context.UserRecipeFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        return FavoriteServiceStatus.Success;
    }

    public async Task<List<RecipeListResponseDto>?> GetMyFavoritesAsync()
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return null;
        }

        var favoriteRecipeIds = _context.UserRecipeFavorites
            .Where(f => f.UserId == userId.Value)
            .Select(f => f.RecipeId);

        return await RecipeReadService
            .WhereVisibleTo(_context.Recipes.AsNoTracking(), userId.Value)
            .Where(r => favoriteRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Id)
            .Select(r => new RecipeListResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                CoffeeId = r.CoffeeId,
                BrewingMethodId = r.BrewingMethodId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null,
                CreatedAt = r.CreatedAt,
                IsFavorite = true
            })
            .ToListAsync();
    }
}
