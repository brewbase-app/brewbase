using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponseDto>>>
        GetNotifications()
    {
        var notifications =
            await _notificationService.GetNotificationsAsync();

        return Ok(notifications);
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync();

        return NoContent();
    }
}