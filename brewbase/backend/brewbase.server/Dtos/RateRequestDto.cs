using System.ComponentModel.DataAnnotations;

namespace brewbase.server.Dtos;

public class RateRequestDto
{
    [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5.")]
    public int Value { get; set; }
}