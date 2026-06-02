using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public class CreateFlavorProfileRequestDto
{
    [Required(ErrorMessage = "Nazwa profilu smakowego jest wymagana.")]
    [MaxLength(50, ErrorMessage = "Nazwa profilu smakowego może mieć maksymalnie 50 znaków.")]
    public string Name { get; set; } = null!;
}
