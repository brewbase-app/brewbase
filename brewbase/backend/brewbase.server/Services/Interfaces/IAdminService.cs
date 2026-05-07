
using brewbase.server.Dtos;
using brewbase.server.Models;
namespace brewbase.server.Services.Interfaces;


public interface IAdminService
{
    Task<List<AdminUserListResponseDto>> GetUsersAsync();
    Task<bool> UpdateUserRoleAsync(int userId, string role);
}