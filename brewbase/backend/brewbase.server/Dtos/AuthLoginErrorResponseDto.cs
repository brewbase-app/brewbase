namespace brewbase.server.Dtos;

public class AuthLoginErrorResponseDto
{
    public string Message { get; set; } = "Nieprawidłowy login lub hasło.";

    public string? PasswordHint { get; set; }
}
