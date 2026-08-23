using StockFlow.Models;

namespace StockFlow.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        RefreshToken GenerateRefreshToken(User user);
    }
}
