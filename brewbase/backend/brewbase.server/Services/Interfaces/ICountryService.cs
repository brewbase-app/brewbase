using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICountryService
{
    Task<List<CountryResponseDto>> GetAllAsync();
}
