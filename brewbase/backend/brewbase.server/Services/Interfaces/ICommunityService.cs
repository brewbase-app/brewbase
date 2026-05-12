using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICommunityService
{
    Task<bool> FollowUserAsync(int followedUserId);

    Task<bool> UnfollowUserAsync(int followedUserId);
    
    Task<FollowStatsResponseDto?> GetFollowStatsAsync();
    Task<PublicUserProfileResponseDto?> GetPublicProfileAsync(int userId);
    
    Task<List<FollowUserListResponseDto>> GetFollowingAsync();
    
}