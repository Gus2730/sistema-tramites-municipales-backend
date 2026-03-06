using SistemaTramites.DTOs;

namespace SistemaTramites.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserFromTokenAsync(string token);
        string GenerateJwtToken(UserDto user, List<string> permissions);
    }
}
