using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using brewbase.server.Services.Interfaces;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("profile_info")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetUserInfoAsync();

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
}