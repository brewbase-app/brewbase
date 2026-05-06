using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IQuickNoteService
{
    Task<QuickNoteResponseDto?> CreateAsync(CreateQuickNoteRequestDto request);

    Task<List<QuickNoteResponseDto>?> GetAllForCurrentUserAsync(string? search);

    Task<QuickNoteResponseDto?> GetByIdForCurrentUserAsync(int id);
}
