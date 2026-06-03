using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public class CreateCountryRequestDto
{
    [Required(ErrorMessage = "Nazwa kraju jest wymagana.")]
    [MaxLength(255, ErrorMessage = "Nazwa kraju może mieć maksymalnie 255 znaków.")]
    public string Name { get; set; } = null!;
}
