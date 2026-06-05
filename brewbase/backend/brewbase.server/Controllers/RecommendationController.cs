using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using brewbase.server.Dtos;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(
        IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecommendations()
    {
        var result =
            await _recommendationService
                .GetRecommendationsAsync();

        return Ok(result);
    }
    
    [HttpPost("summary-feedback")]
    public async Task<IActionResult> SubmitSummaryFeedback(
        RecommendationSummaryFeedbackRequestDto request)
    {
        await _recommendationService.SubmitSummaryFeedbackAsync(request);

        return NoContent();
    }
}