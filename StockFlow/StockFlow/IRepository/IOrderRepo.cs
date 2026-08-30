using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.IRepository
{
    public interface IOrderRepo : IGenericRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(int id);

        Task <List<Order>>GetOrderWithItemsAsync();
    }
}
