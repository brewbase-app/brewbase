using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRoasteryService
{
    Task<List<RoasteryResponseDto>> GetAllAsync();

    Task<List<RoasterySearchResultDto>> SearchAsync(string? query, int limit);

    Task<RoasteryResponseDto> CreateAsync(CreateRoasteryRequestDto dto);
}
