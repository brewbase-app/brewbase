using System.ComponentModel.DataAnnotations;
namespace brewbase.server.Dtos;

public class AuthLoginRequestDto
{
    [Required]
    public string Login { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}