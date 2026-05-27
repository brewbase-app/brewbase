using brewbase.server.Dtos;
using brewbase.server.Services;

namespace brewbase.server.Services.Interfaces;

public interface IRecipeFavoriteService
{
    Task<FavoriteServiceStatus> AddAsync(int recipeId);

    Task<FavoriteServiceStatus> RemoveAsync(int recipeId);

    Task<List<RecipeListResponseDto>?> GetMyFavoritesAsync();
}
