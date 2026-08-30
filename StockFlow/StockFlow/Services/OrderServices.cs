using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using StockFlow.DTOs.Orders;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;
using System.Security.Claims;

namespace StockFlow.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly IOrderRepo _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderServices(IOrderRepo orderRepo, IProductRepository productRepository)
        {
            _orderRepository = orderRepo;
            _productRepository = productRepository;
        }
        //userid will get it from claims
        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, int userid)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new Exception("Order must contain at least one item.");

            var order = new Order
            {
                UserId = userid,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                TotalAmount = 0
            };
            foreach (var item in request.Items)
            {
                if (item.quantity <= 0)
                {
                    throw new Exception("Quantity must be greater than zero.");
                }
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception("product not found");
                }
                if (!product.IsActive)
                {
                    throw new Exception($"Product '{product.Name}' is inactive.");
                }
                var total = product.Price * item.quantity;
                var orderitem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.quantity,
                    UnitPrice = product.Price,
                    TotalPrice = total
                };
                order.OrderItems.Add(orderitem);
                order.TotalAmount += total;
            }
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();
            return new OrderResponse
            {
                Id = order.Id,
                //claims from controller
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.
                Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };
        }
        [Authorize(Roles = "Admin")]
        public async Task<IEnumerable<OrderResponse>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
            });
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var orders = await _orderRepository.GetByIdAsync(id);
            if (orders == null)
                return null;
            return new OrderResponse
            {
                Id = orders.Id,
                UserId = orders.UserId,
                Status = orders.Status,
                TotalAmount = orders.TotalAmount,
                CreatedAt = orders.CreatedAt,
                UpdatedAt = orders.UpdatedAt,
            };
        }

        public async Task<OrderResponse> UpdateAsync(int id, UpdateOrderStatusRequest request)
        {
            var orders = await _orderRepository.GetByIdAsync(id);
            if (orders == null)
                throw new Exception("order not found");
            orders.Status = request.Status;
            _orderRepository.Update(orders);
            await _orderRepository.SaveChangesAsync();
            return new OrderResponse
            {
                Id = orders.Id,
                UserId = orders.UserId,
                Status = orders.Status,
                TotalAmount = orders.TotalAmount,
                CreatedAt = orders.CreatedAt,
                UpdatedAt = orders.UpdatedAt,

            };
        }
        public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(int userid)
        {
            var order = await _orderRepository.GetByUserIdAsync(userid);
            return order.Select(o => new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Items = o.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            });

        }


    }
}