using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ITastingSessionWriteService
{
    Task<TastingSessionResponseDto?> CreateAsync(CreateTastingSessionRequestDto request);
}