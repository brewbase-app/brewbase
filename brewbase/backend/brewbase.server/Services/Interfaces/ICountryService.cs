using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICountryService
{
    Task<List<CountryResponseDto>> GetAllAsync();

    Task<List<CountrySearchResultDto>> SearchAsync(string? query, int limit);

    Task<CountryResponseDto> CreateAsync(CreateCountryRequestDto dto);
}
