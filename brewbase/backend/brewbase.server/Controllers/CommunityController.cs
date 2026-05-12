using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/community")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }
    
    [Authorize]
    [HttpGet("follow-stats")]
    public async Task<ActionResult<FollowStatsResponseDto>> GetFollowStats()
    {
        var stats = await _communityService.GetFollowStatsAsync();

        if (stats == null)
            return Unauthorized();

        return Ok(stats);
    }

    [Authorize]
    [HttpPost("follow/{userId}")]
    public async Task<IActionResult> Follow(int userId)
    {
        try
        {
            var result = await _communityService
                .FollowUserAsync(userId);

            if (!result)
                return Unauthorized();

            return Ok();
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("unfollow/{userId}")]
    public async Task<IActionResult> Unfollow(int userId)
    {
        var result = await _communityService
            .UnfollowUserAsync(userId);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<PublicUserProfileResponseDto>> GetPublicProfile(int userId)
    {
        var profile = await _communityService
            .GetPublicProfileAsync(userId);

        if (profile == null)
            return NotFound();

        return Ok(profile);
    }
    
    [Authorize]
    [HttpGet("following")]
    public async Task<ActionResult<List<FollowUserListResponseDto>>> GetFollowing()
    {
        var users = await _communityService
            .GetFollowingAsync();

        return Ok(users);
    }
}