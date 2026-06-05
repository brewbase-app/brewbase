using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class NotificationEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public NotificationEndpointsTests(CoffeeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(userId: 1);
    }

    [Fact]
    public async Task Unauthenticated_GetNotifications_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_ReturnsUnreadStatus()
    {
        SeedNotifications(
            new Notification
            {
                UserId = 1,
                Content = "Unread notification",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                IsRead = false
            },
            new Notification
            {
                UserId = 1,
                Content = "Read notification",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                IsRead = true
            },
            new Notification
            {
                UserId = 2,
                Content = "Other user notification",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

        var response = await _client.GetAsync("/api/notifications");

        response.EnsureSuccessStatusCode();

        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponseDto>>();

        Assert.NotNull(notifications);
        Assert.Equal(2, notifications!.Count);
        Assert.Contains(notifications, n => n.Content == "Unread notification" && !n.IsRead);
        Assert.Contains(notifications, n => n.Content == "Read notification" && n.IsRead);
    }

    [Fact]
    public async Task GetNotifications_ResolvesLegacyFollowNotificationWithFollowerLogin()
    {
        var followCreatedAt = DateTime.UtcNow.AddMinutes(-3);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Notifications.RemoveRange(context.Notifications);
            context.Follows.RemoveRange(context.Follows);

            context.Follows.Add(new Follow
            {
                FollowerId = 2,
                FollowedId = 1,
                CreatedAt = followCreatedAt
            });

            context.Notifications.Add(new Notification
            {
                UserId = 1,
                Content = "Nowy użytkownik zaczął Cię obserwować.",
                CreatedAt = followCreatedAt,
                IsRead = false
            });

            context.SaveChanges();
        }

        var response = await _client.GetAsync("/api/notifications");

        response.EnsureSuccessStatusCode();

        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponseDto>>();

        Assert.NotNull(notifications);
        Assert.Single(notifications!);
        Assert.Equal("@admin.tester zaczął Cię obserwować.", notifications![0].Content);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            var stored = context.Notifications.Single(n => n.UserId == 1);

            Assert.Equal("@admin.tester zaczął Cię obserwować.", stored.Content);
        }
    }

    [Fact]
    public async Task MarkAllAsRead_SetsUnreadNotificationsToRead()
    {
        SeedNotifications(
            new Notification
            {
                UserId = 1,
                Content = "First unread",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                IsRead = false
            },
            new Notification
            {
                UserId = 1,
                Content = "Second unread",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                IsRead = false
            },
            new Notification
            {
                UserId = 2,
                Content = "Other user unread",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

        var response = await _client.PostAsync("/api/notifications/read", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var userNotifications = context.Notifications
            .Where(n => n.UserId == 1)
            .ToList();

        Assert.All(userNotifications, notification => Assert.True(notification.IsRead));

        var otherUserNotification = context.Notifications
            .Single(n => n.UserId == 2);

        Assert.False(otherUserNotification.IsRead);
    }

    private void SeedNotifications(params Notification[] notifications)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Notifications.RemoveRange(context.Notifications);
        context.Notifications.AddRange(notifications);
        context.SaveChanges();
    }
}
