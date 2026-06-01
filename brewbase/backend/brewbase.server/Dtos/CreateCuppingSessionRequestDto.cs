using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class CreateCuppingSessionRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? SessionDate { get; set; }
}