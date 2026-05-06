using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ITastingSessionWriteService
{
    Task<TastingSessionResponseDto?> CreateAsync(CreateTastingSessionRequestDto request);

	Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> AddCoffeeAsync(
        int sessionId,
        AddCoffeeToTastingSessionRequestDto request);

	Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
    int sessionId,
    int coffeeId,
    UpdateTastingSessionCoffeeNoteRequestDto request);
}