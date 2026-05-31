using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ITastingSessionWriteService
{
    Task<TastingSessionResponseDto?> CreateAsync(CreateTastingSessionRequestDto request);

    Task<TastingSessionWriteResult<TastingSessionResponseDto>> UpdateSessionAsync(
        int sessionId,
        UpdateTastingSessionRequestDto request);

    Task<TastingSessionWriteStatus> DeleteSessionAsync(int sessionId);

	Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> AddCoffeeAsync(
        int sessionId,
        AddCoffeeToTastingSessionRequestDto request);

	Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeAsync(
		int sessionId,
		int sessionCoffeeId,
		UpdateTastingSessionCoffeeRequestDto request);
	
	Task<TastingSessionWriteResult<TastingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
		int sessionId,
		int sessionCoffeeId,
		UpdateTastingSessionCoffeeNoteRequestDto request);

    Task<TastingSessionWriteStatus> DeleteCoffeeAsync(int sessionId, int sessionCoffeeId);
}