namespace brewbase.server.Services;

public static class CoffeeArticleMetadataParser
{
    private const string BeanOriginCountryPrefix = "Kraj pochodzenia ziaren: ";
    private const string VarietyPrefix = "Odmiana: ";
    private const string ProcessingPrefix = "Obróbka ziaren: ";
    private const string FlavorProfilePrefix = "Profil smakowy: ";

    public static string[] ParseFlavorProfiles(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<string>();
        }

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith(FlavorProfilePrefix, StringComparison.Ordinal))
            {
                return ParseFlavorProfileValues(line[FlavorProfilePrefix.Length..]);
            }
        }

        return Array.Empty<string>();
    }

    public static (string? BeanOriginCountry, string? Variety, string? ProcessingMethod, string[] FlavorProfiles) Parse(
        string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (null, null, null, Array.Empty<string>());
        }

        string? beanOriginCountry = null;
        string? variety = null;
        string? processingMethod = null;
        string[] flavorProfiles = Array.Empty<string>();

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith(BeanOriginCountryPrefix, StringComparison.Ordinal))
            {
                beanOriginCountry = line[BeanOriginCountryPrefix.Length..].Trim();
            }
            else if (line.StartsWith(VarietyPrefix, StringComparison.Ordinal))
            {
                variety = line[VarietyPrefix.Length..].Trim();
            }
            else if (line.StartsWith(ProcessingPrefix, StringComparison.Ordinal))
            {
                processingMethod = line[ProcessingPrefix.Length..].Trim();
            }
            else if (line.StartsWith(FlavorProfilePrefix, StringComparison.Ordinal))
            {
                flavorProfiles = ParseFlavorProfileValues(
                    line[FlavorProfilePrefix.Length..]);
            }
        }

        return (beanOriginCountry, variety, processingMethod, flavorProfiles);
    }

    private static string[] ParseFlavorProfileValues(string rawValue)
    {
        return rawValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }
}
