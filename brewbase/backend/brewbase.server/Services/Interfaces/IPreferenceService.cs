using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IPreferenceService
{
    Task SavePreferencesAsync(SaveUserPreferencesRequestDto dto);
}