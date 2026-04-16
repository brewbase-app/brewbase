using brewbase.server.Dtos;
using brewbase.server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeController : ControllerBase
{
    private readonly BrewDbContext _context;

    public CoffeeController(BrewDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? regionId,
        [FromQuery] int? roasteryId,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
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

        var coffees = await query
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

        return Ok(coffees);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var coffee = await _context.Coffees
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
                CreatedByUserId = c.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (coffee == null)
        {
            return NotFound();
        }

        return Ok(coffee);
    }
    
}