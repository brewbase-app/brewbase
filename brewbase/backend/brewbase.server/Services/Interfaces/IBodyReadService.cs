namespace DefaultNamespace;

public interface IBodyReadService
{
    Task<IEnumerable<BodyListResponseDto>> GetAllAsync();

    Task<BodyListResponseDto?> GetByIdAsync(int id);
}