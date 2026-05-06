using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class AddCoffeeToTastingSessionRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CoffeeId must be greater than 0.")]
    public int CoffeeId { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Notes cannot be longer than 1000 characters.")]
    public string? Notes { get; set; }
}