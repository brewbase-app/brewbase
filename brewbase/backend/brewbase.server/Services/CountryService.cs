using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CountryService : ICountryService
{
    private const int MaxSearchLimit = 50;

    private readonly BrewDbContext _context;

    public CountryService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryResponseDto>> GetAllAsync()
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .Select(country => new CountryResponseDto
            {
                Id = country.Id,
                Name = country.Name
            })
            .ToListAsync();
    }

    public async Task<List<CountrySearchResultDto>> SearchAsync(
        string? query,
        int limit)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxSearchLimit);
        var normalizedQuery = SearchTextNormalizer.Normalize(query);

        var countries = await _context.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .ToListAsync();

        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return countries
                .Take(normalizedLimit)
                .Select(country => ToSearchResult(
                    country,
                    normalizedQuery,
                    new FlavorProfileMatchScore(0, false, false)))
                .ToList();
        }

        return countries
            .Select(country =>
            {
                var normalizedName = SearchTextNormalizer.Normalize(country.Name);
                var matchScore = FlavorProfileSimilarityScorer.Score(
                    normalizedQuery,
                    normalizedName);

                return new
                {
                    Country = country,
                    NormalizedName = normalizedName,
                    MatchScore = matchScore,
                    SortRank = matchScore.IsExactMatch
                        ? 3
                        : country.Name.StartsWith(
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
            .ThenBy(entry => entry.Country.Name)
            .Take(normalizedLimit)
            .Select(entry => ToSearchResult(
                entry.Country,
                normalizedQuery,
                entry.MatchScore))
            .ToList();
    }

    public async Task<CountryResponseDto> CreateAsync(CreateCountryRequestDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Nazwa kraju jest wymagana.");
        }

        var existing = await FindByNormalizedNameAsync(normalizedName);

        if (existing != null)
        {
            return existing;
        }

        var country = new Country
        {
            Name = normalizedName
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return new CountryResponseDto
        {
            Id = country.Id,
            Name = country.Name
        };
    }

    private async Task<CountryResponseDto?> FindByNormalizedNameAsync(string name)
    {
        var normalizedInput = SearchTextNormalizer.Normalize(name);

        if (string.IsNullOrEmpty(normalizedInput))
        {
            return null;
        }

        var countries = await _context.Countries
            .AsNoTracking()
            .ToListAsync();

        var match = countries.FirstOrDefault(country =>
            SearchTextNormalizer.Normalize(country.Name) == normalizedInput);

        if (match == null)
        {
            return null;
        }

        return new CountryResponseDto
        {
            Id = match.Id,
            Name = match.Name
        };
    }

    private static CountrySearchResultDto ToSearchResult(
        Country country,
        string normalizedQuery,
        FlavorProfileMatchScore matchScore)
    {
        return new CountrySearchResultDto
        {
            Id = country.Id,
            Name = country.Name,
            SimilarityScore = Math.Round(matchScore.SimilarityScore, 4),
            IsExactMatch = matchScore.IsExactMatch,
            IsFuzzyMatch = matchScore.IsFuzzyMatch,
        };
    }
}
