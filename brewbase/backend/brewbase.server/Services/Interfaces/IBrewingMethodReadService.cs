using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IBrewingMethodReadService
{
    Task<List<BrewingMethodResponseDto>> GetAllAsync();
    Task<BrewingMethodResponseDto?> GetByIdAsync(int id);
}