using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CountryService : ICountryService
{
    private readonly BrewDbContext _context;

    public CountryService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryResponseDto>> GetAllAsync()
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .Select(country => new CountryResponseDto
            {
                Id = country.Id,
                Name = country.Name
            })
            .ToListAsync();
    }
}
