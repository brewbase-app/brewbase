using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class CoffeeFavoriteService : ICoffeeFavoriteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CoffeeFavoriteService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<FavoriteServiceStatus> AddAsync(int coffeeId)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return FavoriteServiceStatus.Unauthorized;
        }

        var coffeeExists = await _context.Coffees.AnyAsync(c => c.Id == coffeeId);
        if (!coffeeExists)
        {
            return FavoriteServiceStatus.NotFound;
        }

        var alreadyFavorite = await _context.UserCoffeeFavorites
            .AnyAsync(f => f.UserId == userId.Value && f.CoffeeId == coffeeId);

        if (!alreadyFavorite)
        {
            _context.UserCoffeeFavorites.Add(new UserCoffeeFavorite
            {
                UserId = userId.Value,
                CoffeeId = coffeeId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });

            await _context.SaveChangesAsync();
        }

        return FavoriteServiceStatus.Success;
    }

    public async Task<FavoriteServiceStatus> RemoveAsync(int coffeeId)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return FavoriteServiceStatus.Unauthorized;
        }

        var favorite = await _context.UserCoffeeFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.CoffeeId == coffeeId);

        if (favorite is not null)
        {
            _context.UserCoffeeFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        return FavoriteServiceStatus.Success;
    }

    public async Task<List<CoffeeListResponseDto>?> GetMyFavoritesAsync()
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return null;
        }

        return await _context.UserCoffeeFavorites
            .AsNoTracking()
            .Where(f => f.UserId == userId.Value)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new CoffeeListResponseDto
            {
                Id = f.Coffee.Id,
                Name = f.Coffee.Name,
                IsVerified = f.Coffee.IsVerified,
                Region = f.Coffee.Region != null ? f.Coffee.Region.Name : null,
                Roastery = f.Coffee.Roastery != null ? f.Coffee.Roastery.Name : null,
                ProcessingMethod = f.Coffee.ProcessingMethod != null ? f.Coffee.ProcessingMethod.Name : null,
                Variety = f.Coffee.Variety != null ? f.Coffee.Variety.Name : null,
                CreatedByUserId = f.Coffee.CreatedByUserId,
                BeanOriginCountry = f.Coffee.Region != null && f.Coffee.Region.Country != null
                    ? f.Coffee.Region.Country.Name
                    : null,
                AverageRating = _context.CoffeeRatings
                    .Where(rating => rating.CoffeeId == f.CoffeeId)
                    .Average(rating => (double?)rating.Value),
                RatingCount = _context.CoffeeRatings
                    .Count(rating => rating.CoffeeId == f.CoffeeId),
                IsFavorite = true
            })
            .ToListAsync();
    }
}
