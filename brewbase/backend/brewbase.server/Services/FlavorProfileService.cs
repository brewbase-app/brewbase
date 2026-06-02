using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class FlavorProfileService : IFlavorProfileService
{
    private const int MaxRandomLimit = 50;

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
        string normalizedName)
    {
        var loweredName = normalizedName.ToLowerInvariant();

        return await _context.FlavorProfiles
            .AsNoTracking()
            .Where(profile => profile.Name.Trim().ToLower() == loweredName)
            .Select(profile => new FlavorProfileResponseDto
            {
                Id = profile.Id,
                Name = profile.Name
            })
            .FirstOrDefaultAsync();
    }
}
