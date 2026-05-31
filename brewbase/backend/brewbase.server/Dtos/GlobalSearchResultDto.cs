namespace brewbase.server.Dtos;

public sealed class GlobalSearchResultDto
{
    public int Id { get; set; }

    /// <summary>Entity kind: coffee, recipe, user, wiki, quick_note, cupping.</summary>
    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Snippet { get; set; }

    /// <summary>Frontend route path (e.g. /wiki/coffees/1).</summary>
    public string Path { get; set; } = null!;

    public double Score { get; set; }
}
