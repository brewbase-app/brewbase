using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IFlavorProfileService
{
    Task<List<FlavorProfileResponseDto>> GetAllAsync();

    Task<List<FlavorProfileResponseDto>> GetRandomAsync(int limit);

    Task<FlavorProfileResponseDto> CreateAsync(CreateFlavorProfileRequestDto dto);
}
