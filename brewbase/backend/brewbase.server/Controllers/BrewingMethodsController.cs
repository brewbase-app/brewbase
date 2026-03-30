using brewbase.server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrewingMethodsController : ControllerBase
{
    private readonly BrewDbContext _context;

    public BrewingMethodsController(BrewDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brewingMethods = await _context.BrewingMethods.ToListAsync();
        return Ok(brewingMethods);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brewingMethod = await _context.BrewingMethods
            .FirstOrDefaultAsync(b => b.Id == id);

        if (brewingMethod == null)
        {
            return NotFound();
        }

        return Ok(brewingMethod);
    }
}