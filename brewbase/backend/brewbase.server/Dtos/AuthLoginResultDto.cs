namespace brewbase.server.Dtos;

public class AuthLoginResultDto
{
    public string? Token { get; init; }

    public string? PasswordHint { get; init; }
}
