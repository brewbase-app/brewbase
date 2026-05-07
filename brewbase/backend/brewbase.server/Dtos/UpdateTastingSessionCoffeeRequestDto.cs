using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class UpdateTastingSessionCoffeeRequestDto
{
    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Range(1, 10)]
    public int? AromaScore { get; set; }

    [Range(1, 10)]
    public int? SweetnessScore { get; set; }

    [Range(1, 10)]
    public int? AcidityScore { get; set; }

    [Range(1, 10)]
    public int? BodyScore { get; set; }

    [MaxLength(1000)]
    public string? FlavorProfileNotes { get; set; }

    public bool? CleanCup { get; set; }

    [Range(1, 10)]
    public int? OverallScore { get; set; }
}