using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class NotificationService : INotificationService
{
    private const string LegacyFollowNotificationContent =
        "Nowy użytkownik zaczął Cię obserwować.";

    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public NotificationService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<NotificationResponseDto>>
        GetNotificationsAsync()
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
        {
            return [];
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var contentUpdated = await ResolveLegacyFollowNotificationsAsync(
            userId.Value,
            notifications);

        if (contentUpdated)
        {
            await _context.SaveChangesAsync();
        }

        return notifications
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToList();
    }

    private async Task<bool> ResolveLegacyFollowNotificationsAsync(
        int userId,
        IReadOnlyList<Notification> notifications)
    {
        var legacyNotifications = notifications
            .Where(notification =>
                string.Equals(
                    notification.Content,
                    LegacyFollowNotificationContent,
                    StringComparison.Ordinal))
            .ToList();

        if (legacyNotifications.Count == 0)
        {
            return false;
        }

        var follows = await _context.Follows
            .AsNoTracking()
            .Where(follow => follow.FollowedId == userId)
            .Join(
                _context.AppUsers,
                follow => follow.FollowerId,
                user => user.Id,
                (follow, user) => new
                {
                    follow.FollowerId,
                    follow.CreatedAt,
                    user.Login
                })
            .ToListAsync();

        if (follows.Count == 0)
        {
            return false;
        }

        var usedFollowKeys = new HashSet<(int FollowerId, long CreatedAtTicks)>();
        var contentUpdated = false;

        foreach (var notification in legacyNotifications)
        {
            var matchedFollow = follows
                .Where(follow =>
                    !usedFollowKeys.Contains((follow.FollowerId, follow.CreatedAt.Ticks)))
                .Select(follow => new
                {
                    Follow = follow,
                    DeltaTicks = Math.Abs(
                        (follow.CreatedAt - notification.CreatedAt).Ticks)
                })
                .Where(match => match.DeltaTicks <= TimeSpan.FromSeconds(5).Ticks)
                .OrderBy(match => match.DeltaTicks)
                .Select(match => match.Follow)
                .FirstOrDefault();

            if (matchedFollow == null || string.IsNullOrWhiteSpace(matchedFollow.Login))
            {
                continue;
            }

            ApplyResolvedFollowNotification(
                notification,
                matchedFollow.Login,
                matchedFollow.FollowerId,
                matchedFollow.CreatedAt.Ticks,
                usedFollowKeys,
                ref contentUpdated);
        }

        var unresolvedNotifications = legacyNotifications
            .Where(notification =>
                string.Equals(
                    notification.Content,
                    LegacyFollowNotificationContent,
                    StringComparison.Ordinal))
            .ToList();

        var remainingFollows = follows
            .Where(follow =>
                !usedFollowKeys.Contains((follow.FollowerId, follow.CreatedAt.Ticks)))
            .OrderByDescending(follow => follow.CreatedAt)
            .ToList();

        foreach (var notification in unresolvedNotifications)
        {
            if (remainingFollows.Count == 0)
            {
                break;
            }

            var matchedFollow = remainingFollows[0];
            remainingFollows.RemoveAt(0);

            if (string.IsNullOrWhiteSpace(matchedFollow.Login))
            {
                continue;
            }

            ApplyResolvedFollowNotification(
                notification,
                matchedFollow.Login,
                matchedFollow.FollowerId,
                matchedFollow.CreatedAt.Ticks,
                usedFollowKeys,
                ref contentUpdated);
        }

        return contentUpdated;
    }

    private static void ApplyResolvedFollowNotification(
        Notification notification,
        string followerLogin,
        int followerId,
        long followCreatedAtTicks,
        ISet<(int FollowerId, long CreatedAtTicks)> usedFollowKeys,
        ref bool contentUpdated)
    {
        usedFollowKeys.Add((followerId, followCreatedAtTicks));
        notification.Content = $"@{followerLogin} zaczął Cię obserwować.";
        contentUpdated = true;
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
        {
            return;
        }

        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
    }
    
}