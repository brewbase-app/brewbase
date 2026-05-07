namespace brewbase.server.Dtos;

public class UserProfileRequestDto
{
    public string Login { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}