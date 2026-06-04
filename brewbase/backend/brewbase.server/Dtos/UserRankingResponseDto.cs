namespace brewbase.server.Dtos;

public class UserRankingResponseDto
{
    public int Position { get; set; }

    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public int ActivityScore { get; set; }

    public int PublicRecipeCount { get; set; }

    public int CoffeeRatingCount { get; set; }

    public int RecipeRatingCount { get; set; }
    
    public int CuppingSessionCount { get; set; }
    
    public int FollowersCount { get; set; }

    public int ReceivedRecipeFavoriteCount { get; set; }

    public int PublishedArticleCount { get; set; }
}