using brewbase.server.Dtos;
using brewbase.server.Models;

namespace brewbase.server.Services.Interfaces;

public interface IAuthService
{
    string GenerateJwt(AppUser user);
    Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterRequestDto dto);
    Task<string?> LoginAsync(AuthLoginRequestDto dto);
}