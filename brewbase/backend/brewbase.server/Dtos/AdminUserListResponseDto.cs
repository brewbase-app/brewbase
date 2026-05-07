namespace brewbase.server.Dtos;

public class AdminUserListResponseDto
{
    public int Id { get; set; }
    public string Login { get; set; } = default!;
    public string Role { get; set; } = default!;
}