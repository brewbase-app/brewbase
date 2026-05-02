using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using brewbase.server.Dtos;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly BrewDbContext _context;

    public AuthService(BrewDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    
    public async Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterRequestDto dto)
    {
        var existsLogin = await _context.AppUsers
            .AnyAsync(u => u.Login == dto.Login);

        if (existsLogin)
            throw new Exception("User with this login already exists");

        var existsEmail = await _context.AppUsers
            .AnyAsync(u => u.Email == dto.Email);

        if (existsEmail)
            throw new Exception("User with this email already exists");

        var user = new AppUser
        {
            Email = dto.Email,
            Login = dto.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            ActivityPoints = 0,
            CreatedAt = DateTime.UtcNow,
            IsBlocked = false,
            Label = null
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        return new AuthRegisterResponseDto
        {
            Id = user.Id,
            Login = user.Login
        };
    }

    public async Task<string?> LoginAsync(AuthLoginRequestDto dto)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Login == dto.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return GenerateJwt(user);
    }
    

    public string GenerateJwt(AppUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("login", user.Login),
            new Claim("role", user.Role)
        };

        var keyString = _config["Jwt:Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("JWT Key is not configured");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyString)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}