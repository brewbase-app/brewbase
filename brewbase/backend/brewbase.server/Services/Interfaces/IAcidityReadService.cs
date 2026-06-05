using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface  IAcidityReadService
{
    Task<IEnumerable<AcidityDto>> GetAllAsync();

    Task<AcidityDto?> GetByIdAsync(int id);
}