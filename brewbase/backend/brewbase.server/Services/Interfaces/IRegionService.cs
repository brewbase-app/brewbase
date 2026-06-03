using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRegionService
{
    Task<List<RegionResponseDto>> GetAllAsync(int? countryId = null);

    Task<List<RegionSearchResultDto>> SearchAsync(
        int countryId,
        string? query,
        int limit);

    Task<RegionResponseDto> CreateAsync(CreateRegionRequestDto dto);
}
