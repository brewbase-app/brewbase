using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class QuickNoteService : IQuickNoteService
{
    private readonly BrewDbContext _context;

    public QuickNoteService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<QuickNoteResponseDto> CreateAsync(int userId, CreateQuickNoteRequestDto request)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var entity = new QuickNote
        {
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        };

        _context.QuickNotes.Add(entity);
        await _context.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task<List<QuickNoteResponseDto>> GetAllAsync(int userId, string? search)
    {
        var query = _context.QuickNotes
            .AsNoTracking()
            .Where(n => n.UserId == userId);

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

    public async Task<QuickNoteResponseDto?> GetByIdAsync(int id, int userId)
    {
        return await _context.QuickNotes
            .AsNoTracking()
            .Where(n => n.Id == id && n.UserId == userId)
            .Select(n => new QuickNoteResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<QuickNoteResponseDto?> UpdateAsync(int id, int userId, UpdateQuickNoteRequestDto request)
    {
        var entity = await _context.QuickNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (entity is null)
        {
            return null;
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        entity.Content = request.Content;
        entity.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var entity = await _context.QuickNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (entity is null)
        {
            return false;
        }

        _context.QuickNotes.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    private static QuickNoteResponseDto ToDto(QuickNote entity) =>
        new()
        {
            Id = entity.Id,
            Content = entity.Content,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
