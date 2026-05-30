namespace brewbase.server.Dtos;

public sealed class RecipeValidationResult
{
    public Dictionary<string, string[]> Errors { get; } = new(StringComparer.Ordinal);

    public bool IsValid => Errors.Count == 0;

    public void AddError(string field, string message)
    {
        if (Errors.TryGetValue(field, out var existing))
        {
            Errors[field] = existing.Concat(new[] { message }).ToArray();
            return;
        }

        Errors[field] = new[] { message };
    }
}
