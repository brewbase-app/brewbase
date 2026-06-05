using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;


public class BodyReadService : IBodyReadService
{
    private readonly BrewDbContext _context;

    public BodyReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BodyListResponseDto>> GetAllAsync()
    {
        return await _context.Bodies
            .Select(x => new BodyListResponseDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    public async Task<BodyListResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Bodies
            .Where(x => x.Id == id)
            .Select(x => new BodyListResponseDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .FirstOrDefaultAsync();
    }
}