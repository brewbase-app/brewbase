namespace brewbase.server.Dtos;

/// <summary>
/// Minimal JSON error payload for simple API messages (e.g. 404 with a short reason).
/// </summary>
public sealed class SimpleErrorResponseDto
{
    public string Message { get; init; } = null!;
}
