using brewbase.server.Dtos;


public interface IUserService
{
    Task<UserProfileResponseDto?> GetUserInfoAsync();
    Task<bool> UpdateUserProfileAsync(UserProfileRequestDto dto);
}