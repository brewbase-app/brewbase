using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AcidityController : ControllerBase
{
    private readonly IAcidityReadService _acidityReadService;

    public AcidityController(
        IAcidityReadService acidityReadService)
    {
        _acidityReadService = acidityReadService;
    }

    [HttpGet("/api/Acidity/")]
    public async Task<IActionResult> GetAll()
    {
        var acidities =
            await _acidityReadService.GetAllAsync();

        return Ok(acidities);
    }

    [HttpGet("/api/Acidity/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var acidity =
            await _acidityReadService.GetByIdAsync(id);

        if (acidity == null)
        {
            return NotFound();
        }

        return Ok(acidity);
    }
}