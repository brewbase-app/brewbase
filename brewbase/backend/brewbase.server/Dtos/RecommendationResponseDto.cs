namespace DefaultNamespace;

public class RecommendationResponseDto
{
    public List<CoffeeRecommendationDto> Coffees { get; set; }
        = [];

    public List<RecipeRecommendationDto> Recipes { get; set; }
        = [];
}