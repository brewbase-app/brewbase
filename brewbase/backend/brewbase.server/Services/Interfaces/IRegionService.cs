using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRegionService
{
    Task<List<RegionResponseDto>> GetAllAsync(int? countryId = null);
}
