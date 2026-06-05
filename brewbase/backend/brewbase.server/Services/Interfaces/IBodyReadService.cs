using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IBodyReadService
{
    Task<IEnumerable<BodyListResponseDto>> GetAllAsync();

    Task<BodyListResponseDto?> GetByIdAsync(int id);
}
