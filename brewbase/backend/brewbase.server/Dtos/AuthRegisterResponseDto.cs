namespace brewbase.server.Dtos;

public class AuthRegisterResponseDto
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Token { get; set; } = null!;
}