namespace brewbase.server.Dtos;

public class PublicUserProfileResponseDto
{
    public int UserId { get; set; }

    public string Login { get; set; } = default!;

    public string? Label { get; set; }

    public int ActivityPoints { get; set; }

    public int FollowersCount { get; set; }

    public int FollowingCount { get; set; }

    public int RecipesCount { get; set; }
}