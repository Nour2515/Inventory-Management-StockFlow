using StockFlow.DTOs;

namespace StockFlow.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        Task RevokeTokenAsync(string refreshToken);

        Task CleanupRefreshTokensAsync();

    }
}
