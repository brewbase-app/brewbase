namespace brewbase.server.Models;

public enum RecipeStatus
{
    Draft,
    Published
}

public static class RecipeStatusExtensions
{
    public static bool ToIsPublic(this RecipeStatus status) =>
        status == RecipeStatus.Published;

    public static RecipeStatus FromIsPublic(bool isPublic) =>
        isPublic ? RecipeStatus.Published : RecipeStatus.Draft;
}
