using System.Security.Claims;
using brewbase.server.Models;
using brewbase.server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace brewbase.server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequestDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Created("", result);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequestDto dto)
    {
        var token = await _authService.LoginAsync(dto);

        if (token == null)
            return Unauthorized("Invalid login or password");

        return Ok(new { token });
    }

    
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userIdRaw = User.FindFirstValue("sub");
        int? userId = null;
        if (userIdRaw != null && int.TryParse(userIdRaw, out var parsedId))
        {
            userId = parsedId;
        }

        var login = User.FindFirstValue("login");
        var role = User.FindFirstValue("role");

        return Ok(new
        {
            userId,
            login,
            role
        });
    }
}