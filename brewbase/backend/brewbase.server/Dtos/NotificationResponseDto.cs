namespace brewbase.server.Dtos;

public class NotificationResponseDto
{
    public int Id { get; set; }

    public string Content { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}