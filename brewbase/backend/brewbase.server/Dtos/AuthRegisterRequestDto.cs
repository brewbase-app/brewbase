using System.ComponentModel.DataAnnotations;
namespace brewbase.server.Dtos;

public class AuthRegisterRequestDto
{
	[Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

	[Required]
    [MaxLength(50)]
    public string Login { get; set; } = null!;

	[Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    [MinLength(3)]
    [MaxLength(255)]
    public string PasswordHint { get; set; } = null!;
}