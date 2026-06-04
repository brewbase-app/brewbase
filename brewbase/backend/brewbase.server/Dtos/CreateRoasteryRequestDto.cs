using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public class CreateRoasteryRequestDto
{
    [Required(ErrorMessage = "Nazwa palarni jest wymagana.")]
    [MaxLength(255, ErrorMessage = "Nazwa palarni może mieć maksymalnie 255 znaków.")]
    public string Name { get; set; } = null!;
}
