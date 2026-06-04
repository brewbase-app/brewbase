namespace brewbase.server.Dtos;

public class RoasterySearchResultDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double SimilarityScore { get; set; }

    public bool IsExactMatch { get; set; }

    public bool IsFuzzyMatch { get; set; }
}
