using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface ICuppingSessionWriteService
{
    Task<CuppingSessionResponseDto?> CreateAsync(CreateCuppingSessionRequestDto request);

    Task<CuppingSessionWriteResult<CuppingSessionResponseDto>> UpdateSessionAsync(
        int sessionId,
        UpdateCuppingSessionRequestDto request);

    Task<CuppingSessionWriteStatus> DeleteSessionAsync(int sessionId);

	Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> AddCoffeeAsync(
        int sessionId,
        AddCoffeeToCuppingSessionRequestDto request);

	Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> UpdateCoffeeAsync(
		int sessionId,
		int sessionCoffeeId,
		UpdateCuppingSessionCoffeeRequestDto request);
	
	Task<CuppingSessionWriteResult<CuppingSessionCoffeeResponseDto>> UpdateCoffeeNoteAsync(
		int sessionId,
		int sessionCoffeeId,
		UpdateCuppingSessionCoffeeNoteRequestDto request);

    Task<CuppingSessionWriteStatus> DeleteCoffeeAsync(int sessionId, int sessionCoffeeId);
}