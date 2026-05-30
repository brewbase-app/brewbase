using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;

namespace brewbase.server.Services.Validation;

public sealed class RecipeValidationService : IRecipeValidationService
{
    private const int TitleMinLength = 3;
    private const int TitleMaxLength = 120;
    private const int StepsMinLength = 5;
    private const int StepsMaxLength = 5000;
    private const decimal MaxCoffeeGrams = 1000m;
    private const decimal MaxWaterMl = 5000m;
    private const decimal MinTemperature = 70m;
    private const decimal MaxTemperature = 100m;

    public RecipeValidationResult ValidateDraft(
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId)
    {
        var result = new RecipeValidationResult();

        if (!HasAnyMeaningfulContent(title, steps, parameters, coffeeId, brewingMethodId))
        {
            result.AddError(string.Empty, "Recipe must contain at least one field before saving as draft.");
        }

        ValidateFieldLengths(title, steps, result, requireTitle: false, requireSteps: false);
        ValidateParameterRanges(parameters, result, requireAll: false);

        return result;
    }

    public RecipeValidationResult ValidateForPublish(
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId)
    {
        var result = new RecipeValidationResult();

        ValidateFieldLengths(title, steps, result, requireTitle: true, requireSteps: true);

        if (coffeeId is null or <= 0)
        {
            result.AddError("CoffeeId", "Coffee selection is required.");
        }

        if (brewingMethodId is null or <= 0)
        {
            result.AddError("BrewingMethodId", "Brewing method is required.");
        }

        if (parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            result.AddError("Parameters", "Brewing parameters are required.");
            return result;
        }

        ValidateParameterRanges(parameters, result, requireAll: true);

        return result;
    }

    private static bool HasAnyMeaningfulContent(
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(steps))
        {
            return true;
        }

        if (coffeeId is > 0 || brewingMethodId is > 0)
        {
            return true;
        }

        return HasMeaningfulParameters(parameters);
    }

    private static void ValidateFieldLengths(
        string? title,
        string? steps,
        RecipeValidationResult result,
        bool requireTitle,
        bool requireSteps)
    {
        var trimmedTitle = title?.Trim();
        var trimmedSteps = steps?.Trim();

        if (requireTitle && string.IsNullOrWhiteSpace(trimmedTitle))
        {
            result.AddError("Title", "Title is required.");
        }
        else if (!string.IsNullOrWhiteSpace(trimmedTitle) && requireTitle)
        {
            if (trimmedTitle!.Length < TitleMinLength)
            {
                result.AddError("Title", $"Title must be at least {TitleMinLength} characters.");
            }

            if (trimmedTitle.Length > TitleMaxLength)
            {
                result.AddError("Title", $"Title must not exceed {TitleMaxLength} characters.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(trimmedTitle) && trimmedTitle!.Length > TitleMaxLength)
        {
            result.AddError("Title", $"Title must not exceed {TitleMaxLength} characters.");
        }

        if (requireSteps && string.IsNullOrWhiteSpace(trimmedSteps))
        {
            result.AddError("Steps", "Steps are required.");
        }
        else if (!string.IsNullOrWhiteSpace(trimmedSteps) && requireSteps)
        {
            if (trimmedSteps!.Length < StepsMinLength)
            {
                result.AddError("Steps", $"Steps must be at least {StepsMinLength} characters.");
            }

            if (trimmedSteps.Length > StepsMaxLength)
            {
                result.AddError("Steps", $"Steps must not exceed {StepsMaxLength} characters.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(trimmedSteps) && trimmedSteps!.Length > StepsMaxLength)
        {
            result.AddError("Steps", $"Steps must not exceed {StepsMaxLength} characters.");
        }
    }

    private static void ValidateParameterRanges(
        JsonElement parameters,
        RecipeValidationResult result,
        bool requireAll)
    {
        if (parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            if (requireAll)
            {
                result.AddError("Parameters", "Brewing parameters are required.");
            }

            return;
        }

        if (parameters.ValueKind != JsonValueKind.Object)
        {
            if (requireAll)
            {
                result.AddError("Parameters", "Brewing parameters must be a JSON object.");
            }

            return;
        }

        var hasCoffee = TryParseCoffeeGrams(parameters, out var coffeeGrams);
        var hasWater = TryParseWaterMl(parameters, out var waterMl);
        var hasTemperature = TryParseTemperature(parameters, out var temperature);
        var hasBrewTime = TryParseBrewTimeSeconds(parameters, out var brewTimeSeconds);

        if (requireAll)
        {
            if (!hasCoffee)
            {
                result.AddError("Parameters.Coffee", "Coffee dose is required.");
            }

            if (!hasWater)
            {
                result.AddError("Parameters.Water", "Water amount is required.");
            }

            if (!hasTemperature)
            {
                result.AddError("Parameters.Temperature", "Water temperature is required.");
            }

            if (!hasBrewTime)
            {
                result.AddError("Parameters.BrewTime", "Brew time is required.");
            }
        }

        if (hasCoffee)
        {
            if (coffeeGrams <= 0)
            {
                result.AddError("Parameters.Coffee", "Coffee dose must be greater than 0.");
            }
            else if (coffeeGrams > MaxCoffeeGrams)
            {
                result.AddError("Parameters.Coffee", $"Coffee dose must not exceed {MaxCoffeeGrams} g.");
            }
        }

        if (hasWater)
        {
            if (waterMl <= 0)
            {
                result.AddError("Parameters.Water", "Water amount must be greater than 0.");
            }
            else if (waterMl > MaxWaterMl)
            {
                result.AddError("Parameters.Water", $"Water amount must not exceed {MaxWaterMl} ml.");
            }
        }

        if (hasTemperature)
        {
            if (temperature < MinTemperature || temperature > MaxTemperature)
            {
                result.AddError(
                    "Parameters.Temperature",
                    $"Temperature must be between {MinTemperature} and {MaxTemperature} °C.");
            }
        }

        if (hasBrewTime && brewTimeSeconds <= 0)
        {
            result.AddError("Parameters.BrewTime", "Brew time must be greater than 0.");
        }
    }

    private static bool HasMeaningfulParameters(JsonElement parameters)
    {
        if (parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return false;
        }

        if (parameters.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        foreach (var property in parameters.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(property.Value.GetString()))
            {
                return true;
            }

            if (property.Value.ValueKind == JsonValueKind.Number &&
                property.Value.TryGetDecimal(out var number) &&
                number > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseCoffeeGrams(JsonElement parameters, out decimal value)
    {
        value = 0;

        if (parameters.TryGetProperty("coffee_grams", out var gramsElement) &&
            TryGetPositiveDecimal(gramsElement, out value))
        {
            return true;
        }

        if (parameters.TryGetProperty("coffee", out var coffeeElement) &&
            coffeeElement.ValueKind == JsonValueKind.String)
        {
            return TryParseAmount(coffeeElement.GetString(), out value);
        }

        return false;
    }

    private static bool TryParseWaterMl(JsonElement parameters, out decimal value)
    {
        value = 0;

        if (parameters.TryGetProperty("water_ml", out var waterElement) &&
            TryGetPositiveDecimal(waterElement, out value))
        {
            return true;
        }

        if (parameters.TryGetProperty("water", out var waterStringElement) &&
            waterStringElement.ValueKind == JsonValueKind.String)
        {
            return TryParseAmount(waterStringElement.GetString(), out value);
        }

        return false;
    }

    private static bool TryParseTemperature(JsonElement parameters, out decimal value)
    {
        value = 0;

        if (parameters.TryGetProperty("temperature", out var temperatureElement))
        {
            if (temperatureElement.ValueKind == JsonValueKind.Number &&
                TryGetPositiveDecimal(temperatureElement, out value))
            {
                return true;
            }

            if (temperatureElement.ValueKind == JsonValueKind.String)
            {
                return TryParseAmount(temperatureElement.GetString(), out value);
            }
        }

        return false;
    }

    private static bool TryParseBrewTimeSeconds(JsonElement parameters, out int value)
    {
        value = 0;

        if (parameters.TryGetProperty("brewTime", out var brewTimeElement) &&
            brewTimeElement.ValueKind == JsonValueKind.String)
        {
            var brewTime = brewTimeElement.GetString();
            if (string.IsNullOrWhiteSpace(brewTime))
            {
                return false;
            }

            var match = Regex.Match(brewTime, @"^(?<minutes>\d+):(?<seconds>\d+)$");
            if (!match.Success)
            {
                return false;
            }

            var minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
            var seconds = int.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);
            value = (minutes * 60) + seconds;
            return true;
        }

        if (parameters.TryGetProperty("brew_time_seconds", out var secondsElement) &&
            secondsElement.TryGetInt32(out value))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetPositiveDecimal(JsonElement element, out decimal value)
    {
        value = 0;

        if (!element.TryGetDecimal(out value))
        {
            return false;
        }

        return value > 0;
    }

    private static bool TryParseAmount(string? rawValue, out decimal value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var normalized = Regex.Replace(rawValue, @"[^\d.,]", string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        normalized = normalized.Replace(',', '.');

        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return false;
        }

        return value > 0;
    }
}
