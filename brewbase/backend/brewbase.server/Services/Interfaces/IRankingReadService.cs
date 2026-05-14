using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRankingReadService
{
    Task<List<CoffeeRankingResponseDto>> GetCoffeeRankingAsync(int limit);
}