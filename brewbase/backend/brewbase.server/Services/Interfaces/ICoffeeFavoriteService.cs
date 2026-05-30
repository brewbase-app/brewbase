using brewbase.server.Dtos;
using brewbase.server.Services;

namespace brewbase.server.Services.Interfaces;

public interface ICoffeeFavoriteService
{
    Task<FavoriteServiceStatus> AddAsync(int coffeeId);

    Task<FavoriteServiceStatus> RemoveAsync(int coffeeId);

    Task<List<CoffeeListResponseDto>?> GetMyFavoritesAsync();
}
