using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;

namespace brewbase.server.Services;

public sealed class TastingSessionWriteService : ITastingSessionWriteService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TastingSessionWriteService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<TastingSessionResponseDto?> CreateAsync(CreateTastingSessionRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return null;
        }

        var tastingSession = new CuppingSession
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            UserId = userId.Value
        };

        _context.CuppingSessions.Add(tastingSession);
        await _context.SaveChangesAsync();

        return new TastingSessionResponseDto
        {
            Id = tastingSession.Id,
            Name = tastingSession.Name,
            Description = tastingSession.Description,
            CreatedAt = tastingSession.CreatedAt,
            UserId = tastingSession.UserId
        };
    }
}