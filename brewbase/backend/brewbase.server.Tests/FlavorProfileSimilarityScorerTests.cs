using brewbase.server.Services;
using Xunit;

namespace brewbase.server.Tests;

public class FlavorProfileSimilarityScorerTests
{
    [Theory]
    [InlineData("jasmin", "jasmin", 1, true, false)]
    [InlineData("jasmni", "jasmin", 0.72, true, true)]
    [InlineData("cytrusy", "cytrusy", 1, true, false)]
    [InlineData("cytrsuzy", "cytrusy", 0.72, true, true)]
    [InlineData("czekolada", "czekolada", 1, true, false)]
    [InlineData("czekoldaa", "czekolada", 0.72, true, true)]
    [InlineData("truskawka", "truskawka", 1, true, false)]
    [InlineData("tuksawka", "truskawka", 0.72, true, true)]
    [InlineData("truskwaka", "truskawka", 0.72, true, true)]
    public void Score_MatchesExpectedTypoCases(
        string query,
        string candidate,
        double minimumSimilarity,
        bool shouldInclude,
        bool shouldBeFuzzy)
    {
        var normalizedQuery = SearchTextNormalizer.Normalize(query);
        var normalizedCandidate = SearchTextNormalizer.Normalize(candidate);

        var score = FlavorProfileSimilarityScorer.Score(
            normalizedQuery,
            normalizedCandidate);

        Assert.True(score.SimilarityScore >= minimumSimilarity);
        Assert.Equal(shouldInclude, FlavorProfileSimilarityScorer.ShouldIncludeInSearch(
            normalizedQuery,
            normalizedCandidate));
        Assert.Equal(shouldBeFuzzy, score.IsFuzzyMatch);
    }

    [Fact]
    public void Score_ExactMatchIsNotFuzzy()
    {
        var score = FlavorProfileSimilarityScorer.Score("jasmin", "jasmin");

        Assert.True(score.IsExactMatch);
        Assert.False(score.IsFuzzyMatch);
        Assert.Equal(1, score.SimilarityScore);
    }

    [Fact]
    public void Score_UnrelatedNamesAreExcluded()
    {
        var normalizedQuery = SearchTextNormalizer.Normalize("karmel");
        var normalizedCandidate = SearchTextNormalizer.Normalize("Jaśmin");

        Assert.False(FlavorProfileSimilarityScorer.ShouldIncludeInSearch(
            normalizedQuery,
            normalizedCandidate));
    }
}
