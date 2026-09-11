using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
