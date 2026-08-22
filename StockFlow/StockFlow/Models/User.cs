using Microsoft.AspNetCore.Identity;
using StockFlow.Models;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public ICollection<WarehouseTransfer> WarehouseTransfers { get; set; } = new List<WarehouseTransfer>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}