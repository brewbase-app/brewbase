using brewbase.server.Models;

namespace brewbase.server.Services.Interfaces;

public interface IAuthService
{
    string GenerateJwt(AppUser user);
}