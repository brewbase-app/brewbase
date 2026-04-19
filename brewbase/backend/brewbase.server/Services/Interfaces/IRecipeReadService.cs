using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRecipeReadService
{
    Task<List<RecipeListResponseDto>> GetAllAsync(
        int? coffeeId,
        int? userId,
        int? brewingMethodId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize);

    Task<RecipeDetailResponseDto?> GetByIdAsync(int id);
}