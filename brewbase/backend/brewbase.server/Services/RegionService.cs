using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RegionService : IRegionService
{
    private readonly BrewDbContext _context;

    public RegionService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<RegionResponseDto>> GetAllAsync(int? countryId = null)
    {
        var query = _context.Regions.AsNoTracking().AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(region => region.CountryId == countryId.Value);
        }

        return await query
            .OrderBy(region => region.Country.Name)
            .ThenBy(region => region.Name)
            .Select(region => new RegionResponseDto
            {
                Id = region.Id,
                Name = region.Name,
                CountryId = region.CountryId
            })
            .ToListAsync();
    }
}
