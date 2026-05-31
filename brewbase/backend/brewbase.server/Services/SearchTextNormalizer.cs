using System.Globalization;
using System.Text;

namespace brewbase.server.Services;

/// <summary>
/// Normalizes text for case-insensitive, diacritic-insensitive matching
/// (e.g. "część" and "czesc"). Used by global search on all database providers.
/// </summary>
public static class SearchTextNormalizer
{
    private static readonly Dictionary<char, char> PolishReplacements = new()
    {
        ['ą'] = 'a', ['ć'] = 'c', ['ę'] = 'e', ['ł'] = 'l', ['ń'] = 'n',
        ['ó'] = 'o', ['ś'] = 's', ['ź'] = 'z', ['ż'] = 'z',
        ['Ą'] = 'a', ['Ć'] = 'c', ['Ę'] = 'e', ['Ł'] = 'l', ['Ń'] = 'n',
        ['Ó'] = 'o', ['Ś'] = 's', ['Ź'] = 'z', ['Ż'] = 'z'
    };

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lowered = value.Trim().ToLowerInvariant();
        var decomposed = lowered.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (PolishReplacements.TryGetValue(character, out var replacement))
            {
                builder.Append(replacement);
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    public static string[] SplitQueryWords(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<string>();
        }

        return query
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(word => word.Length > 0)
            .Distinct()
            .ToArray();
    }

    public static bool MatchesAllWords(IEnumerable<string?> fields, IReadOnlyList<string> normalizedWords)
    {
        if (normalizedWords.Count == 0)
        {
            return false;
        }

        var haystack = Normalize(string.Join(" ", fields.Where(f => !string.IsNullOrWhiteSpace(f))));
        return normalizedWords.All(word => haystack.Contains(word, StringComparison.Ordinal));
    }

    public static double ScoreMatch(IReadOnlyList<string> normalizedWords, string? title, params string?[] extraFields)
    {
        if (normalizedWords.Count == 0)
        {
            return 0;
        }

        var normalizedTitle = Normalize(title);
        var normalizedBody = Normalize(string.Join(" ", extraFields.Where(f => !string.IsNullOrWhiteSpace(f))));
        var combined = string.IsNullOrEmpty(normalizedBody)
            ? normalizedTitle
            : $"{normalizedTitle} {normalizedBody}";

        if (!normalizedWords.All(word => combined.Contains(word, StringComparison.Ordinal)))
        {
            return 0;
        }

        double score = 0;

        foreach (var word in normalizedWords)
        {
            if (normalizedTitle.Contains(word, StringComparison.Ordinal))
            {
                score += normalizedTitle.StartsWith(word, StringComparison.Ordinal) ? 12 : 8;
            }

            if (!string.IsNullOrEmpty(normalizedBody)
                && normalizedBody.Contains(word, StringComparison.Ordinal))
            {
                score += 3;
            }
        }

        if (normalizedTitle == string.Join(' ', normalizedWords))
        {
            score += 5;
        }

        return score;
    }

    public static string BuildSnippet(string? text, int maxLength = 140)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim().Replace('\n', ' ').Replace('\r', ' ');
        while (trimmed.Contains("  ", StringComparison.Ordinal))
        {
            trimmed = trimmed.Replace("  ", " ", StringComparison.Ordinal);
        }

        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength].TrimEnd() + "…";
    }
}
