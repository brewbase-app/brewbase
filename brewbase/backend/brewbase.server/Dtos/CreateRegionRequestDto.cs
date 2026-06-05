using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public class CreateRegionRequestDto
{
    [Required(ErrorMessage = "Nazwa regionu jest wymagana.")]
    [MaxLength(255, ErrorMessage = "Nazwa regionu może mieć maksymalnie 255 znaków.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Identyfikator kraju jest wymagany.")]
    public int CountryId { get; set; }
}
