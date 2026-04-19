using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RecipeReadService : IRecipeReadService
{
    private readonly BrewDbContext _context;

    public RecipeReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecipeListResponseDto>> GetAllAsync(
        int? coffeeId,
        int? userId,
        int? brewingMethodId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize)
    {
        var query = _context.Recipes.AsQueryable();

        if (coffeeId.HasValue)
        {
            query = query.Where(r => r.CoffeeId == coffeeId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (brewingMethodId.HasValue)
        {
            query = query.Where(r => r.BrewingMethodId == brewingMethodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Title != null && EF.Functions.ILike(r.Title, $"%{search}%"));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var isTitleSort = string.Equals(sortBy, "title", StringComparison.OrdinalIgnoreCase);

        query = isTitleSort
            ? (isDesc ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title))
            : query.OrderBy(r => r.Id);

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return await query
            .Select(r => new RecipeListResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null
            })
            .ToListAsync();
    }

    public async Task<RecipeDetailResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Recipes
            .Where(r => r.Id == id)
            .Select(r => new RecipeDetailResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null
            })
            .FirstOrDefaultAsync();
    }
}