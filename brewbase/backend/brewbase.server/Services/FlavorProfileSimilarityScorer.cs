namespace brewbase.server.Services;

/// <summary>
/// Similarity scoring for flavor profile names after SearchTextNormalizer.
/// Uses Damerau-Levenshtein so common typos (transpositions) score higher.
/// Works on any database provider; PostgreSQL pg_trgm is not required.
/// </summary>
public static class FlavorProfileSimilarityScorer
{
    public const double FuzzyMatchThreshold = 0.72;

    public const double ShortQueryFuzzyMatchThreshold = 0.78;

    public const int MinimumFuzzyQueryLength = 3;

    public static FlavorProfileMatchScore Score(
        string normalizedQuery,
        string normalizedCandidate)
    {
        if (string.IsNullOrEmpty(normalizedQuery)
            || string.IsNullOrEmpty(normalizedCandidate))
        {
            return new FlavorProfileMatchScore(0, false, false);
        }

        if (normalizedQuery == normalizedCandidate)
        {
            return new FlavorProfileMatchScore(1, true, false);
        }

        var similarity = CalculateSimilarity(normalizedQuery, normalizedCandidate);
        var startsWith = normalizedCandidate.StartsWith(
            normalizedQuery,
            StringComparison.Ordinal);
        var contains = normalizedCandidate.Contains(
            normalizedQuery,
            StringComparison.Ordinal);

        if (startsWith)
        {
            similarity = Math.Max(similarity, 0.92);
        }
        else if (contains)
        {
            similarity = Math.Max(similarity, 0.82);
        }

        var threshold = GetFuzzyThreshold(normalizedQuery);
        var isFuzzyMatch = similarity >= threshold
            && !startsWith
            && !contains;

        return new FlavorProfileMatchScore(similarity, false, isFuzzyMatch);
    }

    public static bool ShouldIncludeInSearch(
        string normalizedQuery,
        string normalizedCandidate)
    {
        var score = Score(normalizedQuery, normalizedCandidate);

        if (score.IsExactMatch)
        {
            return true;
        }

        if (normalizedCandidate.StartsWith(normalizedQuery, StringComparison.Ordinal)
            || normalizedCandidate.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return true;
        }

        return normalizedQuery.Length >= MinimumFuzzyQueryLength
            && score.SimilarityScore >= GetFuzzyThreshold(normalizedQuery);
    }

    public static double GetFuzzyThreshold(string normalizedQuery)
    {
        return normalizedQuery.Length <= 4
            ? ShortQueryFuzzyMatchThreshold
            : FuzzyMatchThreshold;
    }

    public static double CalculateSimilarity(string left, string right)
    {
        if (left == right)
        {
            return 1;
        }

        if (left.Length == 0 || right.Length == 0)
        {
            return 0;
        }

        var distance = DamerauLevenshteinDistance(left, right);
        var maxLength = Math.Max(left.Length, right.Length);

        return 1 - (double)distance / maxLength;
    }

    public static int DamerauLevenshteinDistance(string source, string target)
    {
        var sourceLength = source.Length;
        var targetLength = target.Length;

        if (sourceLength == 0)
        {
            return targetLength;
        }

        if (targetLength == 0)
        {
            return sourceLength;
        }

        var distances = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
        {
            distances[i, 0] = i;
        }

        for (var j = 0; j <= targetLength; j++)
        {
            distances[0, j] = j;
        }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                distances[i, j] = Math.Min(
                    Math.Min(
                        distances[i - 1, j] + 1,
                        distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);

                if (i > 1
                    && j > 1
                    && source[i - 1] == target[j - 2]
                    && source[i - 2] == target[j - 1])
                {
                    distances[i, j] = Math.Min(
                        distances[i, j],
                        distances[i - 2, j - 2] + cost);
                }
            }
        }

        return distances[sourceLength, targetLength];
    }
}

public readonly record struct FlavorProfileMatchScore(
    double SimilarityScore,
    bool IsExactMatch,
    bool IsFuzzyMatch);
