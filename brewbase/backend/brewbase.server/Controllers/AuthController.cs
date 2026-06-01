using System.Security.Claims;
using brewbase.server.Models;
using brewbase.server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using brewbase.server.Authentication;


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
        try
        {
            var result = await _authService.LoginAsync(dto);

            if (result.Token == null)
            {
                return Unauthorized(new AuthLoginErrorResponseDto
                {
                    PasswordHint = result.PasswordHint
                });
            }

            return Ok(new { token = result.Token });
        }
        catch (Exception ex) when (ex.Message == "User account is blocked")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new SimpleErrorResponseDto
            {
                Message = "Konto użytkownika zostało zablokowane."
            });
        }
    }

    
    //Testowa metoda do sprawdzania czy token który został wygenerowany zwraca odpowiednie dane
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = UserClaims.GetUserId(User),
            login = UserClaims.GetLogin(User),
            role = UserClaims.GetRole(User),
        });
    }
}