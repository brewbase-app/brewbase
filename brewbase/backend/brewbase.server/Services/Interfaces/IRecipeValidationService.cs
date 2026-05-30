using System.Text.Json;
using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IRecipeValidationService
{
    RecipeValidationResult ValidateDraft(
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId);

    RecipeValidationResult ValidateForPublish(
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId);
}
