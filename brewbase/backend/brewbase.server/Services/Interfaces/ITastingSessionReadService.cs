using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ITastingSessionReadService
{
    Task<List<TastingSessionListItemResponseDto>> GetUserSessionsAsync();
    Task<TastingSessionDetailsResponseDto?> GetSessionDetailsAsync(int id);
}