namespace brewbase.server.Dtos;

public class RegionSearchResultDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CountryId { get; set; }

    public double SimilarityScore { get; set; }

    public bool IsExactMatch { get; set; }

    public bool IsFuzzyMatch { get; set; }
}
