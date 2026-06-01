export function getRecipeModerationComment(recipe) {
    if (!recipe) {
        return "";
    }

    return (
        recipe.moderationComment ??
        recipe.ModerationComment ??
        ""
    ).trim();
}
