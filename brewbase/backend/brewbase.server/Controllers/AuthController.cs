using brewbase.server.Models;
using brewbase.server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BrewDbContext _context;

    public AuthController(BrewDbContext context)
    {
        _context = context;
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
        if (existsLogin)
        {
             return Conflict("User with this email already exists");
        }

        var createUser = new AppUser
        {
            Email = dto.Email,
            Login = dto.Login,
            PasswordHash = dto.Password,
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
}