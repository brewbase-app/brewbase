using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class PreferenceService : IPreferenceService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PreferenceService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    /*public async Task SavePreferencesAsync(
        SaveUserPreferencesRequestDto dto)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            throw new Exception("User not found");

        var preference = await _context.UserPreferences
            .Include(p => p.UserPreferenceFlavorProfiles)
            .Include(p => p.UserPreferenceBrewingMethods)
            .Include(p => p.UserPreferenceRegions)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference == null)
        {
            preference = new UserPreference
            {
                UserId = userId.Value,
                QuizCompleted = true
            };

            _context.UserPreferences.Add(preference);
            await _context.SaveChangesAsync();
        }

        preference.ExperienceLevel = dto.ExperienceLevel;
        preference.PreferredRoastLevel = dto.PreferredRoastLevel;
        preference.PreferredAcidity = dto.PreferredAcidity;
        preference.PreferredBody = dto.PreferredBody;
        preference.RecommendationStyle = dto.RecommendationStyle;
        preference.AllowExploration = dto.AllowExploration;
        preference.QuizCompleted = true;

        _context.UserPreferenceFlavorProfiles.RemoveRange(
            preference.UserPreferenceFlavorProfiles);

        _context.UserPreferenceBrewingMethods.RemoveRange(
            preference.UserPreferenceBrewingMethods);

        _context.UserPreferenceRegions.RemoveRange(
            preference.UserPreferenceRegions);

        foreach (var flavorId in dto.FlavorProfileIds)
        {
            preference.UserPreferenceFlavorProfiles.Add(
                new UserPreferenceFlavorProfile
                {
                    FlavorProfileId = flavorId
                });
        }

        foreach (var brewingMethodId in dto.BrewingMethodIds)
        {
            preference.UserPreferenceBrewingMethods.Add(
                new UserPreferenceBrewingMethod
                {
                    BrewingMethodId = brewingMethodId
                });
        }

        foreach (var regionId in dto.RegionIds)
        {
            preference.UserPreferenceRegions.Add(
                new UserPreferenceRegion
                {
                    RegionId = regionId
                });
        }

        await _context.SaveChangesAsync();
    }*/
        public async Task SavePreferencesAsync(
        SaveUserPreferencesRequestDto dto)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            throw new Exception("User not found");

        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference == null)
        {
            preference = new UserPreference
            {
                UserId = userId.Value
            };

            _context.UserPreferences.Add(preference);
        }

        // Pola wymagane
        preference.PreferredRoastLevel = dto.PreferredRoastLevel;
        preference.FavoriteNotes = "";
        preference.QuizCompleted = true;
        preference.AllowExploration = dto.AllowExploration;

        // Pola opcjonalne
        preference.ExperienceLevel = dto.ExperienceLevel;
        preference.PreferredAcidity = dto.PreferredAcidity;
        preference.PreferredBody = dto.PreferredBody;
        preference.RecommendationStyle = dto.RecommendationStyle;

        // Najpierw zapis UserPreference aby mieć wygenerowane ID
        await _context.SaveChangesAsync();

        // Usunięcie starych powiązań
        _context.UserPreferenceFlavorProfiles.RemoveRange(
            _context.UserPreferenceFlavorProfiles
                .Where(x => x.UserPreferenceId == preference.Id));

        _context.UserPreferenceBrewingMethods.RemoveRange(
            _context.UserPreferenceBrewingMethods
                .Where(x => x.UserPreferenceId == preference.Id));

        _context.UserPreferenceRegions.RemoveRange(
            _context.UserPreferenceRegions
                .Where(x => x.UserPreferenceId == preference.Id));

        await _context.SaveChangesAsync();

        foreach (var flavorId in dto.FlavorProfileIds)
        {
            _context.UserPreferenceFlavorProfiles.Add(
                new UserPreferenceFlavorProfile
                {
                    UserPreferenceId = preference.Id,
                    FlavorProfileId = flavorId
                });
        }

        // Dodanie metod parzenia
        foreach (var brewingMethodId in dto.BrewingMethodIds)
        {
            _context.UserPreferenceBrewingMethods.Add(
                new UserPreferenceBrewingMethod
                {
                    UserPreferenceId = preference.Id,
                    BrewingMethodId = brewingMethodId
                });
        }

        // Dodanie regionów
        foreach (var regionId in dto.RegionIds)
        {
            _context.UserPreferenceRegions.Add(
                new UserPreferenceRegion
                {
                    UserPreferenceId = preference.Id,
                    RegionId = regionId
                });
        }

        await _context.SaveChangesAsync();
    }
}