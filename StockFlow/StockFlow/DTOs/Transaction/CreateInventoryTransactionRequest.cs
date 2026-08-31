using StockFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Transaction
{
    public class CreateInventoryTransactionRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public InventoryTransactionType Type { get; set; }
        public string? Reference { get; set; }

    }
}
