namespace brewbase.server.Dtos;

public class AuthRegisterRequestDto
{
    public string Email { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}