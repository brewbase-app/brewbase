using System.Net;
using System.Net.Http.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class CommunityEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public CommunityEndpointsTests(CoffeeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(userId: 1);
    }

    [Fact]
    public async Task GetPublicProfileByLogin_ReturnsProfile()
    {
        ResetFollows();

        var response = await _client.GetAsync("/api/community/profile/by-login/admin.tester");

        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<PublicUserProfileResponseDto>();

        Assert.NotNull(profile);
        Assert.Equal(2, profile!.UserId);
        Assert.Equal("admin.tester", profile.Login);
        Assert.False(profile.IsFollowing);
    }

    [Fact]
    public async Task GetFollowers_ReturnsUsersWhoFollowProfile()
    {
        ResetFollows();

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            context.Follows.Add(new Follow
            {
                FollowerId = 1,
                FollowedId = 2,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }

        var response = await _client.GetAsync("/api/community/followers/2");

        response.EnsureSuccessStatusCode();

        var followers = await response.Content.ReadFromJsonAsync<List<FollowUserListResponseDto>>();

        Assert.NotNull(followers);
        Assert.Single(followers!);
        Assert.Equal(1, followers![0].UserId);
        Assert.Equal("coffee.tester", followers[0].Login);
    }

    [Fact]
    public async Task FollowUser_SetsIsFollowingOnPublicProfile()
    {
        ResetFollows();

        var followResponse = await _client.PostAsync("/api/community/follow/2", null);

        followResponse.EnsureSuccessStatusCode();

        var profileResponse = await _client.GetAsync("/api/community/profile/2");

        profileResponse.EnsureSuccessStatusCode();

        var profile = await profileResponse.Content.ReadFromJsonAsync<PublicUserProfileResponseDto>();

        Assert.NotNull(profile);
        Assert.True(profile!.IsFollowing);
        Assert.Equal(1, profile.FollowersCount);
    }

    [Fact]
    public async Task GetPublicProfileByLogin_ReturnsNotFoundForUnknownLogin()
    {
        var response = await _client.GetAsync("/api/community/profile/by-login/unknown-user");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private void ResetFollows()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        context.Follows.RemoveRange(context.Follows);
        context.SaveChanges();
    }
}
