using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class FlavorProfileService : IFlavorProfileService
{
    private const int MaxRandomLimit = 50;
    private const int MaxOnboardingLimit = 50;
    private const int MaxSearchLimit = 50;

    private readonly BrewDbContext _context;

    public FlavorProfileService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<FlavorProfileResponseDto>> GetAllAsync()
    {
        return await _context.FlavorProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.Name)
            .Select(profile => new FlavorProfileResponseDto
            {
                Id = profile.Id,
                Name = profile.Name
            })
            .ToListAsync();
    }

    public async Task<List<FlavorProfileResponseDto>> GetRandomAsync(int limit)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxRandomLimit);

        var profiles = await _context.FlavorProfiles
            .AsNoTracking()
            .Select(profile => new FlavorProfileResponseDto
            {
                Id = profile.Id,
                Name = profile.Name
            })
            .ToListAsync();

        if (profiles.Count <= normalizedLimit)
        {
            return profiles
                .OrderBy(profile => profile.Name)
                .ToList();
        }

        return profiles
            .OrderBy(_ => Guid.NewGuid())
            .Take(normalizedLimit)
            .ToList();
    }

    public async Task<List<FlavorProfileResponseDto>> GetOnboardingAsync(int limit)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxOnboardingLimit);

        return await _context.FlavorProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.Id)
            .Take(normalizedLimit)
            .Select(profile => new FlavorProfileResponseDto
            {
                Id = profile.Id,
                Name = profile.Name
            })
            .ToListAsync();
    }

    public async Task<List<FlavorProfileSearchResultDto>> SearchAsync(
        string? query,
        int limit)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxSearchLimit);
        var normalizedQuery = SearchTextNormalizer.Normalize(query);

        var profiles = await _context.FlavorProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.Name)
            .ToListAsync();

        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return profiles
                .Take(normalizedLimit)
                .Select(profile => ToSearchResult(
                    profile,
                    normalizedQuery,
                    new FlavorProfileMatchScore(0, false, false)))
                .ToList();
        }

        return profiles
            .Select(profile =>
            {
                var normalizedName = SearchTextNormalizer.Normalize(profile.Name);
                var matchScore = FlavorProfileSimilarityScorer.Score(
                    normalizedQuery,
                    normalizedName);

                return new
                {
                    Profile = profile,
                    NormalizedName = normalizedName,
                    MatchScore = matchScore,
                    SortRank = matchScore.IsExactMatch
                        ? 3
                        : profile.Name.StartsWith(
                            query?.Trim() ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase)
                            || normalizedName.StartsWith(
                                normalizedQuery,
                                StringComparison.Ordinal)
                            ? 2
                            : 1,
                };
            })
            .Where(entry => FlavorProfileSimilarityScorer.ShouldIncludeInSearch(
                normalizedQuery,
                entry.NormalizedName))
            .OrderByDescending(entry => entry.MatchScore.IsExactMatch)
            .ThenByDescending(entry => entry.SortRank)
            .ThenByDescending(entry => entry.MatchScore.SimilarityScore)
            .ThenBy(entry => entry.Profile.Name)
            .Take(normalizedLimit)
            .Select(entry => ToSearchResult(
                entry.Profile,
                normalizedQuery,
                entry.MatchScore))
            .ToList();
    }

    public async Task<FlavorProfileResponseDto> CreateAsync(
        CreateFlavorProfileRequestDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Nazwa profilu smakowego jest wymagana.");
        }

        var existing = await FindByNormalizedNameAsync(normalizedName);

        if (existing != null)
        {
            return existing;
        }

        var profile = new FlavorProfile
        {
            Name = normalizedName
        };

        _context.FlavorProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return new FlavorProfileResponseDto
        {
            Id = profile.Id,
            Name = profile.Name
        };
    }

    private async Task<FlavorProfileResponseDto?> FindByNormalizedNameAsync(
        string name)
    {
        var normalizedInput = SearchTextNormalizer.Normalize(name);

        if (string.IsNullOrEmpty(normalizedInput))
        {
            return null;
        }

        var profiles = await _context.FlavorProfiles
            .AsNoTracking()
            .ToListAsync();

        var match = profiles.FirstOrDefault(profile =>
            SearchTextNormalizer.Normalize(profile.Name) == normalizedInput);

        if (match == null)
        {
            return null;
        }

        return new FlavorProfileResponseDto
        {
            Id = match.Id,
            Name = match.Name
        };
    }

    private static FlavorProfileSearchResultDto ToSearchResult(
        FlavorProfile profile,
        string normalizedQuery,
        FlavorProfileMatchScore matchScore)
    {
        return new FlavorProfileSearchResultDto
        {
            Id = profile.Id,
            Name = profile.Name,
            SimilarityScore = Math.Round(matchScore.SimilarityScore, 4),
            IsExactMatch = matchScore.IsExactMatch,
            IsFuzzyMatch = matchScore.IsFuzzyMatch,
        };
    }
}
