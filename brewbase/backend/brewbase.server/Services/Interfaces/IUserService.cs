using brewbase.server.Dtos;


public interface IUserService
{
    Task<UserProfileResponseDto?> GetUserInfoAsync();
}