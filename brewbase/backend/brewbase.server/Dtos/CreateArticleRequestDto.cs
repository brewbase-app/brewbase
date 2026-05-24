using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class CreateArticleRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Module { get; set; } = null!;
}
