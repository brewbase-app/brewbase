using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class CreateTastingSessionRequestDto
{
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}