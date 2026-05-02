namespace brewbase.server.Dtos;

public class UserProfileResponseDto
{
    public int UserId { get; set; }
    public string Login { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int ActivityPoints { get; set; }
    
}