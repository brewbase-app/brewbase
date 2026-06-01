using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICuppingSessionReadService
{
    Task<List<CuppingSessionListItemResponseDto>> GetUserSessionsAsync(int userId);

    Task<CuppingSessionDetailsResponseDto?> GetSessionDetailsAsync(int id, int userId);
}