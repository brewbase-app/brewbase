using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CoffeeReadService : ICoffeeReadService
{
    private readonly BrewDbContext _context;

    public CoffeeReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoffeeListResponseDto>> GetAllAsync(
        int? regionId,
        int? roasteryId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize)
    {
        var query = _context.Coffees.AsQueryable();

        if (regionId.HasValue)
        {
            query = query.Where(c => c.RegionId == regionId.Value);
        }

        if (roasteryId.HasValue)
        {
            query = query.Where(c => c.RoasteryId == roasteryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name != null && EF.Functions.ILike(c.Name, $"%{search}%"));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var isNameSort = string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase);

        query = isNameSort
            ? (isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name))
            : query.OrderBy(c => c.Id);

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return await query
            .Select(c => new CoffeeListResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsVerified = c.IsVerified,
                Region = c.Region != null ? c.Region.Name : null,
                Roastery = c.Roastery != null ? c.Roastery.Name : null,
                ProcessingMethod = c.ProcessingMethod != null ? c.ProcessingMethod.Name : null,
                Variety = c.Variety != null ? c.Variety.Name : null,
                CreatedByUserId = c.CreatedByUserId
            })
            .ToListAsync();
    }

    public async Task<CoffeeDetailResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Coffees
            .Where(c => c.Id == id)
            .Select(c => new CoffeeDetailResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsVerified = c.IsVerified,
                Region = c.Region != null ? c.Region.Name : null,
                Roastery = c.Roastery != null ? c.Roastery.Name : null,
                ProcessingMethod = c.ProcessingMethod != null ? c.ProcessingMethod.Name : null,
                Variety = c.Variety != null ? c.Variety.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                AverageRating = _context.CoffeeRatings
                    .Where(rating => rating.CoffeeId == c.Id)
                    .Average(rating => (double?)rating.Value),
                RatingCount = _context.CoffeeRatings
                    .Count(rating => rating.CoffeeId == c.Id)
            })
            .FirstOrDefaultAsync();
    }
}