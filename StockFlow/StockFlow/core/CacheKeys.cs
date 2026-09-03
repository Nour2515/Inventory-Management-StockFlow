namespace StockFlow.core
{
    public class CacheKeys
    {
        public const string InventoryAll =
            "inventory:all";

        public static string InventoryById(int id)
        {
            return $"inventory:id:{id}";
        }

        public static string InventoryByProduct(int productId)
        {
            return $"inventory:product:{productId}";
        }

        public static string InventoryByWarehouse(int warehouseId)
        {
            return $"inventory:warehouse:{warehouseId}";
        }
    }
}
