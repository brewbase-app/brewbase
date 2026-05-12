namespace brewbase.server.Dtos;

public class FollowUserListResponseDto
{
    public int UserId { get; set; }

    public string Login { get; set; } = default!;

    public string? Label { get; set; }
}