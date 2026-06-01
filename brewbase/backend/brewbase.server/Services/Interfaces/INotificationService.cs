using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetNotificationsAsync();

    Task MarkAllAsReadAsync();
}