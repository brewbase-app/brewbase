using brewbase.server.Dtos;
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
        var brewingMethods = await _context.BrewingMethods
            .Select(b => new BrewingMethodResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .ToListAsync();

        return Ok(brewingMethods);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brewingMethod = await _context.BrewingMethods
            .Where(b => b.Id == id)
            .Select(b => new BrewingMethodResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .FirstOrDefaultAsync();

        if (brewingMethod == null)
        {
            return NotFound();
        }

        return Ok(brewingMethod);
    }
}