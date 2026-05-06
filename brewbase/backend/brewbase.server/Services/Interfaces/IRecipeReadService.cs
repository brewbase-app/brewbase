using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

/// <summary>
/// Returns recipes visible to the given user (public or owned).
/// Caller must ensure the user is authenticated.
/// </summary>
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
        int? pageSize,
        int currentUserId);

    Task<RecipeDetailResponseDto?> GetByIdAsync(int id, int currentUserId);
}