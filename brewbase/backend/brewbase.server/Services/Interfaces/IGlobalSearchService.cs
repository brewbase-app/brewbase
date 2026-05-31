using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IGlobalSearchService
{
    Task<GlobalSearchResponseDto> SearchAsync(
        int currentUserId,
        string? query,
        int? limit,
        CancellationToken cancellationToken = default);
}
