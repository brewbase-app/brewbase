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
    public async Task<IActionResult> GetAll([FromQuery] int? regionId, [FromQuery] int? roasteryId, [FromQuery] int? page, [FromQuery] int? pageSize)
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

		query = query.OrderBy(c => c.Id);
		
		if (page.HasValue && pageSize.HasValue)
    	{
        	var skip = (page.Value - 1) * pageSize.Value;
        	query = query.Skip(skip).Take(pageSize.Value);
    	}

        var coffees = await query
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.IsVerified,
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
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.IsVerified,
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