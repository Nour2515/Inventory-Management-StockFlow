using StockFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.DTOs.Transaction
{
    public class CreateInventoryTransactionRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be greater than zero.")]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [EnumDataType(typeof(InventoryTransactionType))]
        public InventoryTransactionType Type { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }
    }
}
