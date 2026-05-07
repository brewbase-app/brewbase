using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class UpdateQuickNoteRequestDto
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = null!;
}
