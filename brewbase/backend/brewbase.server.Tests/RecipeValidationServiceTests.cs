using System.Text.Json;
using brewbase.server.Services.Interfaces;
using brewbase.server.Services.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class RecipeValidationServiceTests
{
    private readonly IRecipeValidationService _validationService;

    public RecipeValidationServiceTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRecipeValidationService, RecipeValidationService>();
        _validationService = services.BuildServiceProvider()
            .GetRequiredService<IRecipeValidationService>();
    }

    [Fact]
    public void ValidateForPublish_MissingCoffeeId_ReturnsError()
    {
        var parameters = JsonDocument.Parse(
            """{"coffee":"18g","water":"300ml","temperature":"94","brewTime":"3:30"}""").RootElement;

        var result = _validationService.ValidateForPublish(
            title: "Valid title",
            steps: "Valid steps for publish validation.",
            parameters,
            coffeeId: null,
            brewingMethodId: 1);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("CoffeeId"));
    }

    [Fact]
    public void ValidateForPublish_TemperatureBelowMin_ReturnsError()
    {
        var parameters = JsonDocument.Parse(
            """{"coffee":"18g","water":"300ml","temperature":"65","brewTime":"3:30"}""").RootElement;

        var result = _validationService.ValidateForPublish(
            title: "Valid title",
            steps: "Valid steps for publish validation.",
            parameters,
            coffeeId: 1,
            brewingMethodId: 1);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            pair => pair.Value.Any(message =>
                message.Contains("Temperature", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void ValidateDraft_EmptyContent_ReturnsError()
    {
        var parameters = JsonDocument.Parse("{}").RootElement;

        var result = _validationService.ValidateDraft(
            title: "",
            steps: "",
            parameters,
            coffeeId: null,
            brewingMethodId: null);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey(string.Empty));
    }
}
