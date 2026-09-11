using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs.Reservation;
using StockFlow.DTOs.SignalR;
using StockFlow.Hubs;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;


using StockFlow.Exceptions;

namespace StockFlow.Services
{
    public class StockReservationService : IStockReservationService
    {
        private readonly IStockReservationRepository _stockReservationRepository;

        private readonly IOrderRepo _orderRepo;

        private readonly IProductRepository _productrepo;

        private readonly IGenericRepository<Warehouse> _Warehouserepo;

        private readonly IInventoryRepository _inventoryrepo;

        private readonly IInventoryTransactionRepo _transactionRepository;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<InventoryHub> _hubContext;
        private readonly AppDbContext _context;


        public StockReservationService(IStockReservationRepository stockReservationRepository,IOrderRepo orderRepository,IProductRepository productRepository,IGenericRepository<Warehouse> warehouseRepository,IInventoryRepository inventoryRepository,IInventoryTransactionRepo transactionRepository,ICacheService cacheService,AppDbContext context,IHubContext<InventoryHub> hubContext)
        {
            _stockReservationRepository =stockReservationRepository;

            _orderRepo =orderRepository;

            _productrepo =productRepository;

            _Warehouserepo =warehouseRepository;

            _inventoryrepo =inventoryRepository;

            _transactionRepository =transactionRepository;
            _cacheService =cacheService;
            _hubContext =hubContext;
            _context =context;
        }


        public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request ,int userId)
        {
            if (request.Quantity <= 0)
            {
                throw new ValidationException("Quantity must be greater than zero.");
            }

            var order = await _orderRepo.GetByIdAsync(request.OrderId);

            if (order == null)
                throw new NotFoundException("Order not found.");

            if (order.Status == OrderStatus.Completed ||
       order.Status == OrderStatus.Cancelled)
            {
                throw new ConflictException( $"Cannot create reservation for " +$"{order.Status} order.");
            }


           //product must belong to it

            var orderItem =order.OrderItems.FirstOrDefault(i =>i.ProductId == request.ProductId);

            if (orderItem == null)
            {
                throw new BusinessRuleException("Product does not belong to this order.");
            }


           
            // PREVENT EXCESS RESERVATION

            var activeReservations = await _stockReservationRepository
                    .GetActiveByOrderIdAsync(order.Id);
             
            var alreadyReservedQuantity =activeReservations
                    .Where(r =>r.ProductId == request.ProductId)
                    .Sum(r => r.Quantity);


            var remainingQuantity =orderItem.Quantity -alreadyReservedQuantity;


            if (request.Quantity > remainingQuantity)
            {
                throw new ConflictException($"Reservation quantity exceeds order quantity. " +$"Remaining quantity: {remainingQuantity}.");
            }


            var product =await _productrepo.GetByIdAsync(request.ProductId);

            if (product == null)
                throw new NotFoundException("Product not found.");

            if (!product.IsActive)
                throw new BusinessRuleException("Product is not active.");


            var warehouse =await _Warehouserepo.GetByIdAsync(request.WarehouseId);

            if (warehouse == null)
                throw new NotFoundException("Warehouse not found.");

            if (!warehouse.IsActive)
                throw new BusinessRuleException("Warehouse is inactive.");

            var inventory =await _inventoryrepo.GetByProductAndWarehouseAsync( request.ProductId, request.WarehouseId);

            if (inventory == null)
                throw new NotFoundException("Inventory not found.");

            var availableQuantity =inventory.OnHandQuantity -inventory.ReservedQuantity;

            if (request.Quantity > availableQuantity)
            {
                throw new InsufficientStockException($"Insufficient quantity. Available quantity is: {availableQuantity}");
            }
            StockReservation? stockReservation = null;
            //Transaction 
            await using var dbTransaction =await _context.Database.BeginTransactionAsync();

            try
            { 
                //process 1
                stockReservation =new StockReservation
                    {
                        OrderId =request.OrderId,

                        ProductId =request.ProductId,

                        WarehouseId =request.WarehouseId,

                        Quantity =request.Quantity,

                        Status =ReservationStatus.Active,

                        CreatedAt =DateTime.UtcNow,

                        ExpiresAt =DateTime.UtcNow.AddMinutes(30)
                    };


                await _stockReservationRepository.AddAsync(stockReservation);
                //process 2
                inventory.ReservedQuantity +=request.Quantity;

                inventory.UpdatedAt =DateTime.UtcNow;

                _inventoryrepo.Update(inventory);


                //Process 3
                var inventoryTransaction =new InventoryTransaction
                    {
                        ProductId =request.ProductId,

                        WarehouseId =request.WarehouseId,

                        Quantity =request.Quantity,

                        Type =InventoryTransactionType.Reservation,

                        CreatedAt =DateTime.UtcNow,

                        CreatedByUserId=userId
                };


                await _transactionRepository.AddAsync(inventoryTransaction);

                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();


              
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new ConflictException(
                    "Inventory was modified by another request. Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }

            await _cacheService.InvalidateInventoryCacheAsync(inventory);
            await _hubContext.Clients.All.SendAsync(
             "InventoryUpdated",
             new InventoryUpdatedEvent
             {
                 InventoryId = inventory.Id,

                 ProductId = inventory.ProductId,

                 WarehouseId = inventory.WarehouseId,

                 OnHandQuantity =
                     inventory.OnHandQuantity,

                 ReservedQuantity =
                     inventory.ReservedQuantity,

                 AvailableQuantity =
                     inventory.OnHandQuantity -
                     inventory.ReservedQuantity,

                 Reason = "Reservation"
    });

            var updatedAvailableQuantity =inventory.OnHandQuantity -inventory.ReservedQuantity;

            if (updatedAvailableQuantity <= inventory.ReorderLevel)
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
                            product.Name,

                        WarehouseId =
                            inventory.WarehouseId,

                        WarehouseName =
                            warehouse.Name,

                        AvailableQuantity =
                            updatedAvailableQuantity,

                        ReorderLevel =
                            inventory.ReorderLevel
                    });
            }

            return new ReservationResponse
            {
                Id = stockReservation.Id,

                OrderId = stockReservation.OrderId,

                ProductId = stockReservation.ProductId,

                ProductName = product.Name,

                WarehouseId = stockReservation.WarehouseId,

                WarehouseName = warehouse.Name,

                Quantity = stockReservation.Quantity,

                ExpiresAt = stockReservation.ExpiresAt,

                Status = stockReservation.Status,

                CreatedAt = stockReservation.CreatedAt
            };

        }

        public async Task CancelAsync(int id, int userId)
        {
            var reservation =await _stockReservationRepository.GetByIdWithDetailsAsync(id);

            if (reservation == null)
                throw new NotFoundException("Reservation not found.");

            if (reservation.Status !=ReservationStatus.Active)
            {
                throw new ConflictException("Only active reservations can be cancelled.");
            }


            var inventory =await _inventoryrepo.GetByProductAndWarehouseAsync(reservation.ProductId,reservation.WarehouseId);

            if (inventory == null)
                throw new NotFoundException("Inventory not found.");


            if (inventory.ReservedQuantity <reservation.Quantity)
            {
                throw new BusinessRuleException("Reserved quantity is invalid.");
            }


            await using var dbTransaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {

                inventory.ReservedQuantity -=reservation.Quantity;

                inventory.UpdatedAt =DateTime.UtcNow;


                reservation.Status =ReservationStatus.Cancelled;


                _inventoryrepo.Update(inventory);

                _stockReservationRepository.Update(reservation);


                var inventoryTransaction =
                    new InventoryTransaction
                    {
                        ProductId =
                            reservation.ProductId,

                        WarehouseId =
                            reservation.WarehouseId,

                        Quantity =-reservation.Quantity,

                        Type =
                            InventoryTransactionType
                                .ReservationReleased,

                        CreatedAt =
                            DateTime.UtcNow,

                            CreatedByUserId=userId

                    };


                await _transactionRepository
                    .AddAsync(inventoryTransaction);

                await _context.SaveChangesAsync();


                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new ConflictException(
                    "Inventory was modified by another request. Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                throw;
            }
            await _cacheService.InvalidateInventoryCacheAsync(inventory);

            await _hubContext.Clients.All.SendAsync(
              "InventoryUpdated",
              new InventoryUpdatedEvent
              {
                  InventoryId = inventory.Id,

                  ProductId = inventory.ProductId,

                  WarehouseId = inventory.WarehouseId,

                  OnHandQuantity =
                      inventory.OnHandQuantity,

                  ReservedQuantity =
                      inventory.ReservedQuantity,

                  AvailableQuantity =
                      inventory.OnHandQuantity -
                      inventory.ReservedQuantity,

                  Reason = "ReservationCancelled"
              });
        }

        public async Task ReleaseAsync(int id, int userId)
        {
            var reservation = await _stockReservationRepository.GetByIdWithDetailsAsync(id);

            if (reservation == null)
                throw new NotFoundException("Reservation not found.");

            if (reservation.Status != ReservationStatus.Active)
            {
                throw new ConflictException("Only active reservations can be cancelled.");
            }


            var inventory = await _inventoryrepo.GetByProductAndWarehouseAsync(reservation.ProductId, reservation.WarehouseId);

            if (inventory == null)
                throw new NotFoundException("Inventory not found.");


            if (inventory.ReservedQuantity < reservation.Quantity)
            {
                throw new BusinessRuleException("Reserved quantity is invalid.");
            }


            await using var dbTransaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {

                inventory.ReservedQuantity -= reservation.Quantity;

                inventory.UpdatedAt =DateTime.UtcNow;

                reservation.Status =ReservationStatus.Released;


                _inventoryrepo.Update(inventory);

                _stockReservationRepository.Update(reservation);

                var inventoryTransaction =
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
                        CreatedByUserId = userId

                    };


                await _transactionRepository.AddAsync(inventoryTransaction);


                await _context.SaveChangesAsync();


                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new ConflictException(
                    "Inventory was modified by another request. Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                throw;
            }
            await _cacheService.InvalidateInventoryCacheAsync(inventory);

            await _hubContext.Clients.All.SendAsync(
                 "InventoryUpdated",
                 new InventoryUpdatedEvent
                 {
                     InventoryId = inventory.Id,

                     ProductId = inventory.ProductId,

                     WarehouseId = inventory.WarehouseId,

                     OnHandQuantity =
                         inventory.OnHandQuantity,

                     ReservedQuantity =
                         inventory.ReservedQuantity,

                     AvailableQuantity =
                         inventory.OnHandQuantity -
                         inventory.ReservedQuantity,

                     Reason = "ReservationReleased"
                 });


        }
        public async Task<IEnumerable<ReservationResponse>>GetAllReservations()
        {
            var reservations =await _stockReservationRepository.GetAllAsync();

            return reservations.Select(reservation =>
                new ReservationResponse
                {
                    Id =
                        reservation.Id,

                    OrderId =
                        reservation.OrderId,

                    ProductId =
                        reservation.ProductId,

                    ProductName =
                        reservation.Product.Name,

                    WarehouseId =
                        reservation.WarehouseId,

                    WarehouseName =
                        reservation.Warehouse.Name,

                    Quantity =
                        reservation.Quantity,

                    ExpiresAt =
                        reservation.ExpiresAt,

                    Status =
                        reservation.Status,

                    CreatedAt =
                        reservation.CreatedAt
                });
        }


        public async Task<ReservationResponse?> GetByIdAsync(int id)
        {
            var reservation =await _stockReservationRepository.GetByIdWithDetailsAsync(id);

            if (reservation == null)
                throw new NotFoundException("Reservation not found.");


            return new ReservationResponse
            {
                Id =
                    reservation.Id,

                OrderId =
                    reservation.OrderId,

                ProductId =
                    reservation.ProductId,

                ProductName =
                    reservation.Product.Name,

                WarehouseId =
                    reservation.WarehouseId,

                WarehouseName =
                    reservation.Warehouse.Name,

                Quantity =
                    reservation.Quantity,

                ExpiresAt =
                    reservation.ExpiresAt,

                Status =
                    reservation.Status,

                CreatedAt =
                    reservation.CreatedAt
            };
        }


        public async Task<IEnumerable<ReservationResponse>>GetByOrderIdAsync(int orderId)
        {
            var reservations =await _stockReservationRepository.GetByOrderIdAsync(orderId);

            return reservations.Select(reservation =>
                new ReservationResponse
                {
                    Id =
                        reservation.Id,

                    OrderId =
                        reservation.OrderId,

                    ProductId =
                        reservation.ProductId,

                    ProductName =
                        reservation.Product.Name,

                    WarehouseId =
                        reservation.WarehouseId,

                    WarehouseName =
                        reservation.Warehouse.Name,

                    Quantity =
                        reservation.Quantity,

                    ExpiresAt =
                        reservation.ExpiresAt,

                    Status =
                        reservation.Status,

                    CreatedAt =
                        reservation.CreatedAt
                });
        }


        public async Task ExpireReservationsAsync()
        {
            var expiredReservations =await _stockReservationRepository.GetExpiredActiveAsync();

            if (expiredReservations.Count == 0)
                return;

            var changedInventories = new List<Inventory>();

            await using var dbTransaction =await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var reservation in expiredReservations)
                {
                    var inventory =await _inventoryrepo.GetByProductAndWarehouseAsync(reservation.ProductId,reservation.WarehouseId);

                    if (inventory == null)
                    {
                        throw new NotFoundException("Inventory not found.");
                    }


                    if (inventory.ReservedQuantity < reservation.Quantity)
                    {
                        throw new BusinessRuleException("Reserved quantity is invalid.");
                    }

                    //release stock
                    inventory.ReservedQuantity -= reservation.Quantity;

                    inventory.UpdatedAt =DateTime.UtcNow;


                    reservation.Status =ReservationStatus.Expired;


                    _inventoryrepo.Update(inventory);

                    _stockReservationRepository.Update(reservation);

                    changedInventories.Add(inventory);

                }

                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new ConflictException("Inventory was modified by another request. " + "Please try again.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }


            foreach (var reservation in expiredReservations)
            {
                var inventory =await _inventoryrepo.GetByProductAndWarehouseAsync(reservation.ProductId,reservation.WarehouseId);

                if (inventory != null)
                {
                    await _cacheService.InvalidateInventoryCacheAsync(inventory);
                }
            }
        }
    }
}
