using StockFlow.Models.Enums;

namespace StockFlow.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
}
