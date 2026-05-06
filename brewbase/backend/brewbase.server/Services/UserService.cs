using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class UserService : IUserService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UserService(BrewDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<UserProfileResponseDto?> GetUserInfoAsync()
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            return null;

        var user = await _context.AppUsers
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResponseDto
            {
                UserId = u.Id,
                Login = u.Login,
                Email = u.Email,
                Role = u.Role,
                ActivityPoints = u.ActivityPoints
            })
            .FirstOrDefaultAsync();

        return user;
    }
}