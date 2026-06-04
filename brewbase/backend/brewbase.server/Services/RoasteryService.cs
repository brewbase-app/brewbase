using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RoasteryService : IRoasteryService
{
    private const int MaxSearchLimit = 50;

    private readonly BrewDbContext _context;

    public RoasteryService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoasteryResponseDto>> GetAllAsync()
    {
        return await _context.Roasteries
            .AsNoTracking()
            .OrderBy(roastery => roastery.Name)
            .Select(roastery => new RoasteryResponseDto
            {
                Id = roastery.Id,
                Name = roastery.Name
            })
            .ToListAsync();
    }

    public async Task<List<RoasterySearchResultDto>> SearchAsync(
        string? query,
        int limit)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxSearchLimit);
        var normalizedQuery = SearchTextNormalizer.Normalize(query);

        var roasteries = await _context.Roasteries
            .AsNoTracking()
            .OrderBy(roastery => roastery.Name)
            .ToListAsync();

        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return roasteries
                .Take(normalizedLimit)
                .Select(roastery => ToSearchResult(
                    roastery,
                    normalizedQuery,
                    new FlavorProfileMatchScore(0, false, false)))
                .ToList();
        }

        return roasteries
            .Select(roastery =>
            {
                var normalizedName = SearchTextNormalizer.Normalize(roastery.Name);
                var matchScore = FlavorProfileSimilarityScorer.Score(
                    normalizedQuery,
                    normalizedName);

                return new
                {
                    Roastery = roastery,
                    NormalizedName = normalizedName,
                    MatchScore = matchScore,
                    SortRank = matchScore.IsExactMatch
                        ? 3
                        : roastery.Name.StartsWith(
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
            .ThenBy(entry => entry.Roastery.Name)
            .Take(normalizedLimit)
            .Select(entry => ToSearchResult(
                entry.Roastery,
                normalizedQuery,
                entry.MatchScore))
            .ToList();
    }

    public async Task<RoasteryResponseDto> CreateAsync(CreateRoasteryRequestDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Nazwa palarni jest wymagana.");
        }

        var existing = await FindByNormalizedNameAsync(normalizedName);

        if (existing != null)
        {
            return existing;
        }

        var roastery = new Roastery
        {
            Name = normalizedName
        };

        _context.Roasteries.Add(roastery);
        await _context.SaveChangesAsync();

        return new RoasteryResponseDto
        {
            Id = roastery.Id,
            Name = roastery.Name
        };
    }

    private async Task<RoasteryResponseDto?> FindByNormalizedNameAsync(string name)
    {
        var normalizedInput = SearchTextNormalizer.Normalize(name);

        if (string.IsNullOrEmpty(normalizedInput))
        {
            return null;
        }

        var roasteries = await _context.Roasteries
            .AsNoTracking()
            .ToListAsync();

        var match = roasteries.FirstOrDefault(roastery =>
            SearchTextNormalizer.Normalize(roastery.Name) == normalizedInput);

        if (match == null)
        {
            return null;
        }

        return new RoasteryResponseDto
        {
            Id = match.Id,
            Name = match.Name
        };
    }

    private static RoasterySearchResultDto ToSearchResult(
        Roastery roastery,
        string normalizedQuery,
        FlavorProfileMatchScore matchScore)
    {
        return new RoasterySearchResultDto
        {
            Id = roastery.Id,
            Name = roastery.Name,
            SimilarityScore = Math.Round(matchScore.SimilarityScore, 4),
            IsExactMatch = matchScore.IsExactMatch,
            IsFuzzyMatch = matchScore.IsFuzzyMatch,
        };
    }
}
