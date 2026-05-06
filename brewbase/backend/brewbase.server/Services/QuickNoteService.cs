using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class QuickNoteService : IQuickNoteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuickNoteService(BrewDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<QuickNoteResponseDto?> CreateAsync(CreateQuickNoteRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return null;
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var entity = new QuickNote
        {
            Content = request.Content.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId.Value
        };

        _context.QuickNotes.Add(entity);
        await _context.SaveChangesAsync();

        return new QuickNoteResponseDto
        {
            Id = entity.Id,
            Content = entity.Content,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<List<QuickNoteResponseDto>?> GetAllForCurrentUserAsync(string? search)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return null;
        }

        var query = _context.QuickNotes
            .AsNoTracking()
            .Where(n => n.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(n => EF.Functions.ILike(n.Content, $"%{term}%"));
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new QuickNoteResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<QuickNoteResponseDto?> GetByIdForCurrentUserAsync(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return null;
        }

        return await _context.QuickNotes
            .AsNoTracking()
            .Where(n => n.Id == id && n.UserId == userId.Value)
            .Select(n => new QuickNoteResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }
}
