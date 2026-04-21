using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class BrewingMethodReadService : IBrewingMethodReadService
{
    private readonly BrewDbContext _context;

    public BrewingMethodReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrewingMethodResponseDto>> GetAllAsync()
    {
        return await _context.BrewingMethods
            .Select(b => new BrewingMethodResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .ToListAsync();
    }

    public async Task<BrewingMethodResponseDto?> GetByIdAsync(int id)
    {
        return await _context.BrewingMethods
            .Where(b => b.Id == id)
            .Select(b => new BrewingMethodResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .FirstOrDefaultAsync();
    }
}