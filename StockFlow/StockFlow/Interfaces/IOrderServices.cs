using StockFlow.DTOs.Orders;
using StockFlow.DTOs.Product;

namespace StockFlow.Interfaces
{
    public interface IOrderServices
    {
        Task<OrderResponse> CreateAsync(CreateOrderRequest request, int userid);

        Task<IEnumerable<OrderResponse>> GetAllAsync();

        Task<OrderResponse?> GetByIdAsync(int id);

        Task<OrderResponse> UpdateAsync(int id,UpdateOrderStatusRequest request,int userid);

        Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(int userid);

    }
}
