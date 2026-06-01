using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public sealed class AddCoffeeToCuppingSessionRequestDto
{
    public int? CoffeeId { get; set; }

    [MaxLength(255)]
    public string? CoffeeName { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}