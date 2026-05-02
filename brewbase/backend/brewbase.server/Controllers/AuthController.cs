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
    private readonly BrewDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(BrewDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequestDto dto)
    {
        
        //Sprawdzenie czy user istnieje o takim loginie
        var existsLogin = await _context.AppUsers
            .AnyAsync(login => login.Login == dto.Login);
        
        //Jeżeli taki user istnieje zwróć błąd
        if (existsLogin)
        {
            return Conflict("User with this login already exists");
        }
		
		//Sprawdzenie czy email już nie istnieje
		var existsEmail = await _context.AppUsers
            .AnyAsync(email => email.Email == dto.Email);

		//Jeżeli taki email istnieje zwróć błąd
        if (existsEmail)
        {
             return Conflict("User with this email already exists");
        }

        var createUser = new AppUser
        {
            Email = dto.Email,
            Login = dto.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            ActivityPoints = 0,
            CreatedAt = DateTime.Now,
            IsBlocked = false,
            Label = null
        };

        _context.AppUsers.Add(createUser);
        await _context.SaveChangesAsync();

        return Ok(new AuthRegisterResponseDto
        {
            Id = createUser.Id,
            Login = createUser.Login
        });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequestDto dto)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Login == dto.Login);

        //Jeżeli użytkownik nie istnieje lub hasło jest nie poprawnie to błąd
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid login or password");
        }

        var token = _authService.GenerateJwt(user);

        return Ok(new
        {
            token = token
        });
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