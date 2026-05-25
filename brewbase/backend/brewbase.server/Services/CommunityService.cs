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

    //Follow user
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
        
        _context.Notifications.Add(new Notification
        {
            UserId = followedUserId,
            Content = "Nowy użytkownik zaczął Cię obserwować.",
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return true;
    }

    //UnFollow
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
    
    //Stats numbers
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
    
    //Public profile, get info about user
    public async Task<PublicUserProfileResponseDto?> GetPublicProfileAsync(int userId)
    {
        var user = await _context.AppUsers
            .Where(u => u.Id == userId)
            .Select(u => new PublicUserProfileResponseDto
            {
                UserId = u.Id,
                Login = u.Login,
                Label = u.Label,
                ActivityPoints = u.ActivityPoints,

                FollowersCount = _context.Follows
                    .Count(f => f.FollowedId == u.Id),

                FollowingCount = _context.Follows
                    .Count(f => f.FollowerId == u.Id),

                RecipesCount = _context.Recipes
                    .Count(r => r.UserId == u.Id)
            })
            .FirstOrDefaultAsync();

        return user;
    }
    
    //following lists
    public async Task<List<FollowUserListResponseDto>> GetFollowingAsync()
    {
        var currentUserId = _currentUserProvider.GetUserId();

        if (currentUserId == null)
            return [];

        return await _context.Follows
            .Where(f => f.FollowerId == currentUserId.Value)
            .Select(f => new FollowUserListResponseDto
            {
                UserId = f.Followed.Id,
                Login = f.Followed.Login,
                Label = f.Followed.Label
            })
            .ToListAsync();
    }
    
    public async Task<List<UserActivityResponseDto>> GetFeedAsync()
    {
        var currentUserId = _currentUserProvider.GetUserId();

        var followedUserIds = await _context.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        var articleActivities = await _context.Articles
            .Where(a => followedUserIds.Contains(a.UserId))
            .Select(a => new UserActivityResponseDto
            {
                Username = a.User.Login,
                ActivityType = "Article",
                Description = $"Dodał artykuł: {a.Title}",
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var followActivities = await _context.Follows
            .Where(f => followedUserIds.Contains(f.FollowerId))
            .Select(f => new UserActivityResponseDto
            {
                Username = f.Follower.Login,
                ActivityType = "Follow",
                Description =
                    $"Zaczął obserwować użytkownika {f.Followed.Login}",
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return articleActivities
            .Concat(followActivities)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToList();
    }

}