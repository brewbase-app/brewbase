using brewbase.server.Services;
using Xunit;

namespace brewbase.server.Tests;

public class SearchTextNormalizerTests
{
    [Theory]
    [InlineData("część", "czesc")]
    [InlineData("JAŚMIN", "jasmin")]
    [InlineData("  Etiopia  ", "etiopia")]
    public void Normalize_StripsDiacriticsAndCase(string input, string expected)
    {
        Assert.Equal(expected, SearchTextNormalizer.Normalize(input));
    }

    [Fact]
    public void SplitQueryWords_DeduplicatesAndNormalizes()
    {
        var words = SearchTextNormalizer.SplitQueryWords("  Alpha   alpha  ");

        Assert.Equal(new[] { "alpha" }, words);
    }

    [Fact]
    public void ScoreMatch_ReturnsZeroWhenAnyWordMissing()
    {
        var words = SearchTextNormalizer.SplitQueryWords("alpha beta");

        var score = SearchTextNormalizer.ScoreMatch(words, "Alpha Coffee", "Only alpha here");

        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreMatch_RewardsTitlePrefixMatch()
    {
        var words = SearchTextNormalizer.SplitQueryWords("alpha");

        var score = SearchTextNormalizer.ScoreMatch(words, "Alpha Coffee", "body");

        Assert.True(score >= 12);
    }

    [Fact]
    public void BuildSnippet_TruncatesLongText()
    {
        var snippet = SearchTextNormalizer.BuildSnippet(new string('a', 200), maxLength: 50);

        Assert.EndsWith("…", snippet);
        Assert.True(snippet.Length <= 51);
    }
}
