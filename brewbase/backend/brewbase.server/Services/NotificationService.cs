using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class NotificationService : INotificationService
{
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

        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync();
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