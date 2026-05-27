using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICoffeeReadService
{
    Task<List<CoffeeListResponseDto>> GetAllAsync(
        int? regionId,
        int? roasteryId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize,
        int? currentUserId = null);

    Task<CoffeeDetailResponseDto?> GetByIdAsync(int id, int? currentUserId = null);
}