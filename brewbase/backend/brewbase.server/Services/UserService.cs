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

    public async Task<bool> UpdateUserProfileAsync(UserProfileRequestDto dto)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            return false;

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        var loginExists = await _context.AppUsers
            .AnyAsync(u => u.Login == dto.Login && u.Id != userId);

        if (loginExists)
            throw new Exception("Login already taken");

        var emailExists = await _context.AppUsers
            .AnyAsync(u => u.Email == dto.Email && u.Id != userId);

        if (emailExists)
            throw new Exception("Email already taken");

        user.Login = dto.Login;
        user.Email = dto.Email;

        var wantsPasswordChange =
            !string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(dto.NewPassword);

        if (wantsPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new Exception("Current password is required");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new Exception("New password is required");

            var currentPasswordMatches = BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.PasswordHash
            );

            if (!currentPasswordMatches)
                throw new Exception("Current password is invalid");

            var newPasswordMatchesCurrent = BCrypt.Net.BCrypt.Verify(
                dto.NewPassword,
                user.PasswordHash
            );

            if (newPasswordMatchesCurrent)
                throw new Exception("New password must be different from current password");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _context.SaveChangesAsync();

        return true;
    }
}