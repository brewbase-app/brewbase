using brewbase.server.Dtos;
using DefaultNamespace;

namespace brewbase.server.Services.Interfaces;

public interface IPreferenceService
{
    Task SavePreferencesAsync(SaveUserPreferencesRequestDto dto);
    
    Task<UserPreferencesDto?> GetPreferencesAsync();
}