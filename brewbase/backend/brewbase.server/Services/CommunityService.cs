using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CommunityService : ICommunityService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CommunityService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<bool> FollowUserAsync(int followedUserId)
    {
        var currentUserId = _currentUserProvider.GetUserId();

        if (currentUserId == null)
            return false;

        if (currentUserId == followedUserId)
            throw new Exception("You cannot follow yourself");

        var userExists = await _context.AppUsers
            .AnyAsync(u => u.Id == followedUserId);

        if (!userExists)
            throw new Exception("User not found");

        var alreadyFollowing = await _context.Follows
            .AnyAsync(f =>
                f.FollowerId == currentUserId.Value &&
                f.FollowedId == followedUserId);

        if (alreadyFollowing)
            throw new Exception("User already followed");

        var follow = new Follow
        {
            FollowerId = currentUserId.Value,
            FollowedId = followedUserId,
            CreatedAt = DateTime.Now
        };

        _context.Follows.Add(follow);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnfollowUserAsync(int followedUserId)
    {
        var currentUserId = _currentUserProvider.GetUserId();

        if (currentUserId == null)
            return false;

        var follow = await _context.Follows
            .FirstOrDefaultAsync(f =>
                f.FollowerId == currentUserId.Value &&
                f.FollowedId == followedUserId);

        if (follow == null)
            return false;

        _context.Follows.Remove(follow);

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<FollowStatsResponseDto?> GetFollowStatsAsync()
    {
        var currentUserId = _currentUserProvider.GetUserId();

        if (currentUserId == null)
            return null;

        var followersCount = await _context.Follows
            .CountAsync(f => f.FollowedId == currentUserId);

        var followingCount = await _context.Follows
            .CountAsync(f => f.FollowerId == currentUserId);

        return new FollowStatsResponseDto
        {
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };
    }
}