using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class AcidityReadService : IAcidityReadService
{
    private readonly BrewDbContext _context;

    public AcidityReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AcidityDto>> GetAllAsync()
    {
        return await _context.Acidities
            .Select(x => new AcidityDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    public async Task<AcidityDto?> GetByIdAsync(int id)
    {
        return await _context.Acidities
            .Where(x => x.Id == id)
            .Select(x => new AcidityDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .FirstOrDefaultAsync();
    }
}