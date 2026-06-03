using Microsoft.AspNetCore.Mvc;

namespace DefaultNamespace;

public class BodyController : ControllerBase
{
    private readonly IBodyReadService _bodyReadService;

    public BodyController(
        IBodyReadService bodyReadService)
    {
        _bodyReadService = bodyReadService;
    }

    [HttpGet("/api/Body")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(
            await _bodyReadService.GetAllAsync()
        );
    }
    
    [HttpGet("/api/Body/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var body = await _bodyReadService.GetByIdAsync(id);

        if (body == null)
        {
            return NotFound();
        }

        return Ok(body);
    }
    
}