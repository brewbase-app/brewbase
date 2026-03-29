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
    public async Task<IActionResult> GetAll()
    {
        var coffees = await _context.Coffees
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
}