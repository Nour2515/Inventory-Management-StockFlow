using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs.Orders;
using StockFlow.DTOs.SignalR;
using StockFlow.Hubs;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;

using StockFlow.Exceptions;

namespace StockFlow.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly IOrderRepo _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockReservationRepository _stockReservationRepository;
        private readonly IInventoryTransactionRepo _inventoryTransactionRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<InventoryHub> _hubContext;
        private readonly AppDbContext _context;


        public OrderServices(IOrderRepo orderRepo, IProductRepository productRepository, IInventoryTransactionRepo inventoryTransactionRepo, IStockReservationRepository stockReservationRepository, IInventoryRepository inventoryRepository, ICacheService cacheService, IHubContext<InventoryHub> hubContext, AppDbContext context)
        {
            _orderRepository = orderRepo;
            _productRepository = productRepository;
            _inventoryTransactionRepository = inventoryTransactionRepo;
            _stockReservationRepository = stockReservationRepository;
            _inventoryRepository = inventoryRepository;
            _cacheService = cacheService;
            _hubContext = hubContext;
            _context = context;
        }
        //userid will get it from claims
        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, int userid)
        {   
            if (request.Items == null || request.Items.Count == 0)
                throw new ValidationException("Order must contain at least one item.");
            if (request.Items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
                throw new ConflictException("The same product cannot appear more than once in the same order.");

            var products = new Dictionary<int, Product>();


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
                    throw new ValidationException("Quantity must be greater than zero.");
                }
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new NotFoundException("Product not found.");
                }
                if (!product.IsActive)
                {
                    throw new BusinessRuleException($"Product '{product.Name}' is inactive.");
                }
                products[item.ProductId] = product;
            }
            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];
                var total = product.Price * item.quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.quantity,
                    UnitPrice = product.Price,
                    TotalPrice = total
                });

                order.TotalAmount += total;

            }
            var changedInventories = new List<Inventory>();

            await using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();
                foreach (var orderItem in order.OrderItems)
                {
                    var inventory = await _inventoryRepository.GetAvailableInventoryAsync(orderItem.ProductId, orderItem.Quantity);
                    if (inventory == null)
                    {
                        throw new InsufficientStockException($"Insufficient available stock for product '{products[orderItem.ProductId].Name}'. " + "No active warehouse can fully satisfy the requested quantity.");
                    }
                    var reservation = new StockReservation
                    {
                        OrderId = order.Id,
                        ProductId = orderItem.ProductId,
                        WarehouseId = inventory.WarehouseId,
                        Quantity = orderItem.Quantity,
                        Status = ReservationStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                    };
                    await _stockReservationRepository.AddAsync(reservation);

                    inventory.ReservedQuantity += orderItem.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    _inventoryRepository.Update(inventory);

                    changedInventories.Add(inventory);

                    await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
                    {
                        ProductId = orderItem.ProductId,
                        WarehouseId = inventory.WarehouseId,
                        Quantity = orderItem.Quantity,
                        Type = InventoryTransactionType.Reservation,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userid

                    });

                }
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();
                throw new ConflictException("Inventory was modified by another request. Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
            foreach (var inventory in changedInventories.DistinctBy(i => i.Id))
            {
                await _cacheService.InvalidateInventoryCacheAsync(inventory);


                await _hubContext.Clients.All.SendAsync(
                    "InventoryUpdated",
                    new InventoryUpdatedEvent
                    {
                        InventoryId =
                            inventory.Id,

                        ProductId =
                            inventory.ProductId,

                        WarehouseId =
                            inventory.WarehouseId,

                        OnHandQuantity =
                            inventory.OnHandQuantity,

                        ReservedQuantity =
                            inventory.ReservedQuantity,

                        AvailableQuantity =
                            inventory.OnHandQuantity -
                            inventory.ReservedQuantity,

                        Reason = "OrderCreated"

                    });
                var availableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity;
                if (availableQuantity <= inventory.ReorderLevel)
                {
                    await _hubContext.Clients.All.SendAsync(
                    "LowStockAlert",
            new
            {
                InventoryId =
                    inventory.Id,

                ProductId =
                    inventory.ProductId,

                ProductName =
                    products[inventory.ProductId].Name,

                WarehouseId =
                    inventory.WarehouseId,

                AvailableQuantity =
                    availableQuantity,

                ReorderLevel =
                    inventory.ReorderLevel
            });
                }
            }

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = products[item.ProductId].Name,
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
                throw new NotFoundException("Order not found.");
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

        public async Task<OrderResponse> UpdateAsync(int id,UpdateOrderStatusRequest request,int userid)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("Order not found.");

            if (order.Status == request.Status)
                throw new ConflictException($"Order is already {order.Status}.");

            if (order.Status == OrderStatus.Completed)
                throw new ConflictException("Completed order cannot be updated.");

            if (order.Status == OrderStatus.Cancelled)
                throw new ConflictException("Cancelled order cannot be updated.");


            // Allowed transitions
            bool validTransition = order.Status switch
            {
                OrderStatus.Pending =>request.Status == OrderStatus.Confirmed ||request.Status == OrderStatus.Cancelled,

                OrderStatus.Confirmed =>request.Status == OrderStatus.Completed ||request.Status == OrderStatus.Cancelled,

                _ => false
            };

            if (!validTransition)
            {
                throw new BusinessRuleException($"Invalid order status transition from " +$"{order.Status} to {request.Status}.");
            }

            var changedInventories = new List<Inventory>();


            await using var dbTransaction =await _context.Database.BeginTransactionAsync();

            try
            {
                // CANCEL ORDER

                if (request.Status == OrderStatus.Cancelled)
                {
                    var reservations =await _stockReservationRepository.GetActiveByOrderIdAsync(order.Id);

                    foreach (var reservation in reservations)
                    {
                        var inventory =await _inventoryRepository.GetByProductAndWarehouseAsync(reservation.ProductId,reservation.WarehouseId);

                        if (inventory == null)
                            throw new NotFoundException("Inventory not found.");

                        if (inventory.ReservedQuantity <reservation.Quantity)
                        {
                            throw new BusinessRuleException("Reserved quantity is invalid.");
                        }

                        // فك الحجز
                        inventory.ReservedQuantity -=reservation.Quantity;

                        inventory.UpdatedAt = DateTime.UtcNow;

                        _inventoryRepository.Update(inventory);


                        reservation.Status =ReservationStatus.Cancelled;

                        _stockReservationRepository.Update(reservation);


                        await _inventoryTransactionRepository.AddAsync(
                                new InventoryTransaction
                                {
                                    ProductId =
                                        reservation.ProductId,

                                    WarehouseId =
                                        reservation.WarehouseId,

                                    Quantity =
                                        -reservation.Quantity,

                                    Type =
                                        InventoryTransactionType
                                            .ReservationReleased,

                                    CreatedAt =
                                        DateTime.UtcNow,

                                   CreatedByUserId=userid
                                });
                    }
                }


                 //completed orders

                if (request.Status == OrderStatus.Completed)
                {
                    var reservations =await _stockReservationRepository.GetActiveByOrderIdAsync(order.Id);

                    if (reservations.Count == 0)
                    {
                        throw new ConflictException("Order has no active reservations.");
                    }


                    foreach (var reservation in reservations)
                    {
                        var inventory =await _inventoryRepository.GetByProductAndWarehouseAsync(reservation.ProductId,reservation.WarehouseId);

                        if (inventory == null)
                            throw new NotFoundException("Inventory not found.");


                        if (inventory.ReservedQuantity <reservation.Quantity)
                        {
                            throw new BusinessRuleException("Reserved quantity is invalid.");
                        }


                        if (inventory.OnHandQuantity < reservation.Quantity)
                        {
                            throw new InsufficientStockException(
                                "Insufficient on-hand quantity.");
                        }


                        // الكمية المحجوزة أصبحت مباعة
                        inventory.ReservedQuantity -=reservation.Quantity;

                        inventory.OnHandQuantity -=reservation.Quantity;

                        inventory.UpdatedAt =DateTime.UtcNow;

                        _inventoryRepository.Update(inventory);

                        reservation.Status =ReservationStatus.Released;

                        _stockReservationRepository.Update(reservation);


                         //update in history
                        await _inventoryTransactionRepository
                            .AddAsync(
                                new InventoryTransaction
                                {
                                    ProductId =
                                        reservation.ProductId,

                                    WarehouseId =
                                        reservation.WarehouseId,

                                    Quantity =
                                        -reservation.Quantity,

                                    Type =
                                        InventoryTransactionType
                                            .ReservationReleased,

                                    CreatedAt =
                                        DateTime.UtcNow,
                                    CreatedByUserId = userid
                                });


                        // Sale
                        await _inventoryTransactionRepository
                            .AddAsync(
                                new InventoryTransaction
                                {
                                    ProductId =
                                        reservation.ProductId,

                                    WarehouseId =
                                        reservation.WarehouseId,

                                    Quantity =
                                        -reservation.Quantity,

                                    Type =
                                        InventoryTransactionType.Sale,

                                    CreatedAt =
                                        DateTime.UtcNow,
                                    CreatedByUserId = userid



                                });
                    }
                }

                //update status
                order.Status = request.Status;

                order.UpdatedAt =DateTime.UtcNow;

                _orderRepository.Update(order);

                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new ConflictException("Order or inventory was modified by another request. Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                throw;
            }
            foreach (var inventory in changedInventories.DistinctBy(i => i.Id))
            {
                await _cacheService
                    .InvalidateInventoryCacheAsync(
                        inventory);


                await _hubContext.Clients.All.SendAsync(
                    "InventoryUpdated",
                    new InventoryUpdatedEvent
                    {
                        InventoryId =
                            inventory.Id,

                        ProductId =
                            inventory.ProductId,

                        WarehouseId =
                            inventory.WarehouseId,

                        OnHandQuantity =
                            inventory.OnHandQuantity,

                        ReservedQuantity =
                            inventory.ReservedQuantity,

                        AvailableQuantity =
                            inventory.OnHandQuantity -
                            inventory.ReservedQuantity,

                        Reason =
                            request.Status ==
                                OrderStatus.Cancelled
                                ? "OrderCancelled"
                                : "OrderCompleted"
                    });
            }

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
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
