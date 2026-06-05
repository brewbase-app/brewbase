using brewbase.server.Dtos;
using DefaultNamespace;

namespace brewbase.server.Services.Interfaces;

public interface  IRecommendationService
{
    Task<RecommendationResponseDto> GetRecommendationsAsync();
    
    Task SubmitSummaryFeedbackAsync(RecommendationSummaryFeedbackRequestDto request);
}