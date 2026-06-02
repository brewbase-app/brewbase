using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/preferences")]
[Authorize]
public class PreferenceController : ControllerBase
{
    private readonly IPreferenceService _preferenceService;

    public PreferenceController(IPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    [HttpPost]
    public async Task<IActionResult> SavePreferences(SaveUserPreferencesRequestDto dto)
    {
        await _preferenceService.SavePreferencesAsync(dto);

        return Ok();
    }
}