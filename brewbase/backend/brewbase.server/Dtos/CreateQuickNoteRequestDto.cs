using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class CreateQuickNoteRequestDto
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = null!;
}
