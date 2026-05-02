using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class UpdateTastingSessionCoffeeNoteRequestDto
{
    [MaxLength(1000, ErrorMessage = "Note cannot be longer than 1000 characters.")]
    public string? Notes { get; set; }
}