namespace brewbase.server.Dtos;

public sealed class QuickNoteResponseDto
{
    public int Id { get; init; }

    public string Content { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
