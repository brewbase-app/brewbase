using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class CreateTastingSessionRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}