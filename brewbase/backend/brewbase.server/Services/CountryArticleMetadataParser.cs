namespace brewbase.server.Services;

public static class CountryArticleMetadataParser
{
    private const string RegionPrefix = "Region: ";

    public static (string? Region, string[] FlavorProfiles) Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (null, Array.Empty<string>());
        }

        string? region = null;

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith(RegionPrefix, StringComparison.Ordinal))
            {
                region = line[RegionPrefix.Length..].Trim();
            }
        }

        var flavorProfiles = CoffeeArticleMetadataParser.ParseFlavorProfiles(content);

        return (region, flavorProfiles);
    }
}
