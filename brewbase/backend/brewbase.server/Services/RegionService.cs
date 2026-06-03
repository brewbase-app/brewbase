using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RegionService : IRegionService
{
    private const int MaxSearchLimit = 50;

    private readonly BrewDbContext _context;

    public RegionService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<RegionResponseDto>> GetAllAsync(int? countryId = null)
    {
        var query = _context.Regions.AsNoTracking().AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(region => region.CountryId == countryId.Value);
        }

        return await query
            .OrderBy(region => region.Country.Name)
            .ThenBy(region => region.Name)
            .Select(region => new RegionResponseDto
            {
                Id = region.Id,
                Name = region.Name,
                CountryId = region.CountryId
            })
            .ToListAsync();
    }

    public async Task<List<RegionSearchResultDto>> SearchAsync(
        int countryId,
        string? query,
        int limit)
    {
        await EnsureCountryExistsAsync(countryId);

        var normalizedLimit = Math.Clamp(limit, 1, MaxSearchLimit);
        var normalizedQuery = SearchTextNormalizer.Normalize(query);

        var regions = await _context.Regions
            .AsNoTracking()
            .Where(region => region.CountryId == countryId)
            .OrderBy(region => region.Name)
            .ToListAsync();

        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return regions
                .Take(normalizedLimit)
                .Select(region => ToSearchResult(
                    region,
                    normalizedQuery,
                    new FlavorProfileMatchScore(0, false, false)))
                .ToList();
        }

        return regions
            .Select(region =>
            {
                var normalizedName = SearchTextNormalizer.Normalize(region.Name);
                var matchScore = FlavorProfileSimilarityScorer.Score(
                    normalizedQuery,
                    normalizedName);

                return new
                {
                    Region = region,
                    NormalizedName = normalizedName,
                    MatchScore = matchScore,
                    SortRank = matchScore.IsExactMatch
                        ? 3
                        : region.Name.StartsWith(
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
            .ThenBy(entry => entry.Region.Name)
            .Take(normalizedLimit)
            .Select(entry => ToSearchResult(
                entry.Region,
                normalizedQuery,
                entry.MatchScore))
            .ToList();
    }

    public async Task<RegionResponseDto> CreateAsync(CreateRegionRequestDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Nazwa regionu jest wymagana.");
        }

        await EnsureCountryExistsAsync(dto.CountryId);

        var existing = await FindByNormalizedNameAsync(normalizedName, dto.CountryId);

        if (existing != null)
        {
            return existing;
        }

        var region = new Region
        {
            Name = normalizedName,
            CountryId = dto.CountryId
        };

        _context.Regions.Add(region);
        await _context.SaveChangesAsync();

        return new RegionResponseDto
        {
            Id = region.Id,
            Name = region.Name,
            CountryId = region.CountryId
        };
    }

    private async Task EnsureCountryExistsAsync(int countryId)
    {
        var exists = await _context.Countries
            .AsNoTracking()
            .AnyAsync(country => country.Id == countryId);

        if (!exists)
        {
            throw new ArgumentException("Kraj o podanym identyfikatorze nie istnieje.");
        }
    }

    private async Task<RegionResponseDto?> FindByNormalizedNameAsync(
        string name,
        int countryId)
    {
        var normalizedInput = SearchTextNormalizer.Normalize(name);

        if (string.IsNullOrEmpty(normalizedInput))
        {
            return null;
        }

        var regions = await _context.Regions
            .AsNoTracking()
            .Where(region => region.CountryId == countryId)
            .ToListAsync();

        var match = regions.FirstOrDefault(region =>
            SearchTextNormalizer.Normalize(region.Name) == normalizedInput);

        if (match == null)
        {
            return null;
        }

        return new RegionResponseDto
        {
            Id = match.Id,
            Name = match.Name,
            CountryId = match.CountryId
        };
    }

    private static RegionSearchResultDto ToSearchResult(
        Region region,
        string normalizedQuery,
        FlavorProfileMatchScore matchScore)
    {
        return new RegionSearchResultDto
        {
            Id = region.Id,
            Name = region.Name,
            CountryId = region.CountryId,
            SimilarityScore = Math.Round(matchScore.SimilarityScore, 4),
            IsExactMatch = matchScore.IsExactMatch,
            IsFuzzyMatch = matchScore.IsFuzzyMatch,
        };
    }
}
