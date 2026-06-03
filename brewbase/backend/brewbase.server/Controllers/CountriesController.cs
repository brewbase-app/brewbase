using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var countries = await _countryService.GetAllAsync();
        return Ok(countries);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 20)
    {
        var countries = await _countryService.SearchAsync(q, limit);
        return Ok(countries);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateCountryRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var country = await _countryService.CreateAsync(dto);
            return Ok(country);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new SimpleErrorResponseDto { Message = ex.Message });
        }
    }
}
