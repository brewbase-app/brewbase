using System.Text.Json;
using System.Text.Json.Serialization;

namespace brewbase.server.Services;

internal static class ReportStatuses
{
    public const string Open = "open";
    public const string Dismissed = "dismissed";
    public const string Upheld = "upheld";
}

internal sealed class ReportPayload
{
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = default!;

    [JsonPropertyName("contentId")]
    public int ContentId { get; set; }

    [JsonPropertyName("contentTitle")]
    public string ContentTitle { get; set; } = default!;

    [JsonPropertyName("category")]
    public string Category { get; set; } = default!;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = ReportStatuses.Open;

    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [JsonPropertyName("resolvedByLogin")]
    public string? ResolvedByLogin { get; set; }

    [JsonPropertyName("resolutionAction")]
    public string? ResolutionAction { get; set; }
}

internal static class ReportReasonHelper
{
    private const string Prefix = "@@BBRPT:v1@@";
    private const string LegacySeparator = "\n\n---\n\n";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly IReadOnlyList<string> AllowedCategories =
    [
        "Dezinformacja",
        "Spam lub reklama",
        "Obraźliwa treść",
        "Niebezpieczne instrukcje",
        "Fałszywe informacje",
        "Naruszenie praw autorskich",
        "Duplikat treści",
        "spam",
        "Harassment / nękanie",
        "Inny problem"
    ];

    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "article",
            "recipe",
            "coffee"
        };

    public const int MaxCommentLength = 1000;

    public static string Encode(ReportPayload payload)
    {
        return Prefix + JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static ReportPayload Parse(
        string reason,
        int articleId,
        string? articleTitleFallback = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return CreateLegacyArticlePayload(
                articleId,
                articleTitleFallback,
                "Inny problem",
                null);
        }

        if (reason.StartsWith(Prefix, StringComparison.Ordinal))
        {
            var payload = JsonSerializer.Deserialize<ReportPayload>(
                reason[Prefix.Length..],
                JsonOptions);

            if (payload != null &&
                !string.IsNullOrWhiteSpace(payload.ContentType) &&
                !string.IsNullOrWhiteSpace(payload.Category))
            {
                if (string.IsNullOrWhiteSpace(payload.Status))
                {
                    payload.Status = ReportStatuses.Open;
                }

                return payload;
            }
        }

        var (category, comment) = ParseLegacyCategoryComment(reason);

        return CreateLegacyArticlePayload(
            articleId,
            articleTitleFallback,
            category,
            comment);
    }

    public static bool IsOpen(ReportPayload payload)
    {
        return string.IsNullOrWhiteSpace(payload.Status) ||
               payload.Status == ReportStatuses.Open;
    }

    public static bool IsResolved(ReportPayload payload)
    {
        return payload.Status is ReportStatuses.Dismissed or ReportStatuses.Upheld;
    }

    private static ReportPayload CreateLegacyArticlePayload(
        int articleId,
        string? articleTitleFallback,
        string category,
        string? comment)
    {
        return new ReportPayload
        {
            ContentType = "article",
            ContentId = articleId,
            ContentTitle = string.IsNullOrWhiteSpace(articleTitleFallback)
                ? $"Artykuł #{articleId}"
                : articleTitleFallback.Trim(),
            Category = category,
            Comment = comment,
            Status = ReportStatuses.Open
        };
    }

    private static (string Category, string? Comment) ParseLegacyCategoryComment(
        string reason)
    {
        var separatorIndex = reason.IndexOf(LegacySeparator, StringComparison.Ordinal);

        if (separatorIndex >= 0)
        {
            var category = reason[..separatorIndex].Trim();
            var comment = reason[(separatorIndex + LegacySeparator.Length)..].Trim();

            return (
                string.IsNullOrWhiteSpace(category) ? "Inny problem" : category,
                string.IsNullOrWhiteSpace(comment) ? null : comment);
        }

        return (reason.Trim(), null);
    }
}
