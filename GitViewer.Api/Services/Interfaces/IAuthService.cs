using GitViewer.Api.Dto;
using GitViewer.DataAccess.Models;

namespace GitViewer.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(string request);
        Task<User> DeleteRefreshTokenAsync(User user);
        Task<User> GetUserByRefreshTokenAsync(string refreshToken);
    }
}
