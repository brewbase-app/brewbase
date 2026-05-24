namespace brewbase.server.Dtos;

public class UserActivityResponseDto
{
    public string Username { get; set; } = default!;

    public string ActivityType { get; set; } = default!;

    public string Description { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}