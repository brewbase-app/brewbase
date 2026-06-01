using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CoffeeReadService : ICoffeeReadService
{
    private const string ApprovedStatus = "Approved";
    private const string CoffeeModule = "coffee";

    private readonly BrewDbContext _context;

    public CoffeeReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoffeeListResponseDto>> GetAllAsync(
        int? regionId,
        int? roasteryId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize,
        int? currentUserId = null)
    {
        var query = _context.Coffees.AsQueryable();

        if (regionId.HasValue)
        {
            query = query.Where(c => c.RegionId == regionId.Value);
        }

        if (roasteryId.HasValue)
        {
            query = query.Where(c => c.RoasteryId == roasteryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name != null && EF.Functions.ILike(c.Name, $"%{search}%"));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var isNameSort = string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase);

        query = isNameSort
            ? (isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name))
            : query.OrderBy(c => c.Id);

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        var coffees = await query
            .Select(c => new CoffeeListResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsVerified = c.IsVerified,
                Region = c.Region != null ? c.Region.Name : null,
                Roastery = c.Roastery != null ? c.Roastery.Name : null,
                ProcessingMethod = c.ProcessingMethod != null ? c.ProcessingMethod.Name : null,
                Variety = c.Variety != null ? c.Variety.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                BeanOriginCountry = c.Region != null && c.Region.Country != null
                    ? c.Region.Country.Name
                    : null,
                AverageRating = _context.CoffeeRatings
                    .Where(rating => rating.CoffeeId == c.Id)
                    .Average(rating => (double?)rating.Value),
                RatingCount = _context.CoffeeRatings
                    .Count(rating => rating.CoffeeId == c.Id),
                IsFavorite = currentUserId.HasValue
                    && _context.UserCoffeeFavorites.Any(f =>
                        f.UserId == currentUserId.Value && f.CoffeeId == c.Id)
            })
            .ToListAsync();

        await EnrichListFromLinkedWikiArticlesAsync(coffees);

        return coffees;
    }

    public async Task<CoffeeDetailResponseDto?> GetByIdAsync(int id, int? currentUserId = null)
    {
        var coffee = await _context.Coffees
            .Where(c => c.Id == id)
            .Select(c => new CoffeeDetailResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsVerified = c.IsVerified,
                Region = c.Region != null ? c.Region.Name : null,
                Roastery = c.Roastery != null ? c.Roastery.Name : null,
                ProcessingMethod = c.ProcessingMethod != null ? c.ProcessingMethod.Name : null,
                Variety = c.Variety != null ? c.Variety.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                AverageRating = _context.CoffeeRatings
                    .Where(rating => rating.CoffeeId == c.Id)
                    .Average(rating => (double?)rating.Value),
                RatingCount = _context.CoffeeRatings
                    .Count(rating => rating.CoffeeId == c.Id),
                IsFavorite = currentUserId.HasValue
                    && _context.UserCoffeeFavorites.Any(f =>
                        f.UserId == currentUserId.Value && f.CoffeeId == c.Id)
            })
            .FirstOrDefaultAsync();

        if (coffee is null)
        {
            return null;
        }

        coffee.WikiArticle = await _context.Articles
            .AsNoTracking()
            .Where(article =>
                article.CoffeeId == id
                && article.Module == CoffeeModule
                && article.Status == ApprovedStatus)
            .OrderByDescending(article => article.PublishedAt ?? article.CreatedAt)
            .Select(article => new LinkedCoffeeArticleDto
            {
                Id = article.Id,
                Content = article.Content,
                AuthorLogin = article.User.Login,
                PublishedAt = article.PublishedAt
            })
            .FirstOrDefaultAsync();

        return coffee;
    }

    public async Task<List<CoffeeLookupResponseDto>> LookupByNameAsync(string name, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new List<CoffeeLookupResponseDto>();
        }

        var trimmedName = name.Trim();
        var clampedLimit = Math.Clamp(limit, 1, 20);
        var lowerName = trimmedName.ToLower();

        return await _context.Coffees
            .AsNoTracking()
            .Where(coffee =>
                coffee.Name != null
                && coffee.Name.ToLower().Contains(lowerName))
            .OrderBy(coffee => coffee.Name)
            .Take(clampedLimit)
            .Select(coffee => new CoffeeLookupResponseDto
            {
                Id = coffee.Id,
                Name = coffee.Name
            })
            .ToListAsync();
    }

    private async Task EnrichListFromLinkedWikiArticlesAsync(List<CoffeeListResponseDto> coffees)
    {
        if (coffees.Count == 0)
        {
            return;
        }

        var coffeeIds = coffees.Select(coffee => coffee.Id).ToList();

        var linkedArticles = await _context.Articles
            .AsNoTracking()
            .Where(article =>
                article.Module == CoffeeModule
                && article.Status == ApprovedStatus
                && article.CoffeeId != null
                && coffeeIds.Contains(article.CoffeeId.Value))
            .Select(article => new
            {
                CoffeeId = article.CoffeeId!.Value,
                article.Content
            })
            .ToListAsync();

        foreach (var coffee in coffees)
        {
            var linkedArticle = linkedArticles
                .FirstOrDefault(article => article.CoffeeId == coffee.Id);

            if (linkedArticle is null)
            {
                continue;
            }

            var (beanOriginCountry, variety, processingMethod, flavorProfiles, _) =
                CoffeeArticleMetadataParser.Parse(linkedArticle.Content);

            if (!string.IsNullOrWhiteSpace(beanOriginCountry))
            {
                coffee.BeanOriginCountry = beanOriginCountry;
            }

            if (string.IsNullOrWhiteSpace(coffee.Variety) && !string.IsNullOrWhiteSpace(variety))
            {
                coffee.Variety = variety;
            }

            if (string.IsNullOrWhiteSpace(coffee.ProcessingMethod)
                && !string.IsNullOrWhiteSpace(processingMethod))
            {
                coffee.ProcessingMethod = processingMethod;
            }

            coffee.FlavorProfiles = flavorProfiles.ToList();
        }
    }
}
