using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ITastingSessionReadService
{
    Task<List<TastingSessionListItemResponseDto>> GetUserSessionsAsync(int userId);

    Task<TastingSessionDetailsResponseDto?> GetSessionDetailsAsync(int id, int userId);
}