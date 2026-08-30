namespace StockFlow.DTOs.warehouse
{
    public class WarehouseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string location { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
