using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserListResponseDto>>> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();

        return Ok(users);
    }
    
    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] AdminUpdateUserRoleDto dto)
    {
        try
        {
            var updated = await _adminService.UpdateUserRoleAsync(id, dto.Role);

            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}