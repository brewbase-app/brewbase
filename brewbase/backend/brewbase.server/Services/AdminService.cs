using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class AdminService : IAdminService
{
    private readonly BrewDbContext _context;

    public AdminService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminUserListResponseDto>> GetUsersAsync()
    {
        return await _context.AppUsers
            .Select(u => new AdminUserListResponseDto
            {
                Id = u.Id,
                Login = u.Login,
                Role = u.Role,
            })
            .ToListAsync();
    }
    
    public async Task<bool> UpdateUserRoleAsync(int userId, string role)
    {
        var allowedRoles = new[] { "Admin", "User" };

        if (!allowedRoles.Contains(role))
            throw new Exception("Invalid role");

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        user.Role = role;

        await _context.SaveChangesAsync();

        return true;
    }
}