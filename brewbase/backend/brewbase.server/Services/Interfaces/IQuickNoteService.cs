using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IQuickNoteService
{
    Task<QuickNoteResponseDto> CreateAsync(int userId, CreateQuickNoteRequestDto request);

    Task<List<QuickNoteResponseDto>> GetAllAsync(int userId, string? search);

    Task<QuickNoteResponseDto?> GetByIdAsync(int id, int userId);

    Task<QuickNoteResponseDto?> UpdateAsync(int id, int userId, UpdateQuickNoteRequestDto request);

    Task<bool> DeleteAsync(int id, int userId);
}
