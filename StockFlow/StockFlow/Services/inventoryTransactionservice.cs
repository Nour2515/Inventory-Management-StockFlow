using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs.SignalR;
using StockFlow.DTOs.Transaction;
using StockFlow.Hubs;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;
using StockFlow.Models.Enums;
using System.Runtime.CompilerServices;

namespace StockFlow.Services
{
    public class inventoryTransactionservice : IinventoryTransactionservice
    {
        private readonly IInventoryTransactionRepo _transactionRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly ICacheService _cacheService;

        private readonly IHubContext<InventoryHub> _hubContext;
        private readonly AppDbContext _context;

        public inventoryTransactionservice(IInventoryTransactionRepo transactionRepository,IInventoryRepository inventoryRepository,IProductRepository productRepository,IGenericRepository<Warehouse> warehouseRepository,ICacheService cacheService,AppDbContext context,IHubContext<InventoryHub> hubContext)
        {
            _transactionRepository = transactionRepository;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _cacheService = cacheService;
            _context = context;
            _hubContext = hubContext;
        }


        public async Task<IEnumerable<InventoryTransactionResponse>> GetAllAsync()
        {
            var transactions =await _transactionRepository.GetAllAsync();

            return transactions.Select(transaction =>
                new InventoryTransactionResponse
                {
                    Id = transaction.Id,

                    ProductId = transaction.ProductId,
                    ProductName = transaction.Product.Name,

                    WarehouseId = transaction.WarehouseId,
                    WarehouseName = transaction.Warehouse.Name,

                    Quantity = transaction.Quantity,

                    Type = transaction.Type,

                    Reference = transaction.ReferenceId,

                    CreatedAt = transaction.CreatedAt
                });
        }


        public async Task<InventoryTransactionResponse?> GetByIdAsync(int id)
        {
            var transaction =
                await _transactionRepository.GetByIdAsync(id);

            if (transaction == null)
                return null;

            return new InventoryTransactionResponse
            {
                Id = transaction.Id,

                ProductId = transaction.ProductId,
                ProductName = transaction.Product.Name,

                WarehouseId = transaction.WarehouseId,
                WarehouseName = transaction.Warehouse.Name,

                Quantity = transaction.Quantity,

                Type = transaction.Type,

                Reference = transaction.ReferenceId,

                CreatedAt = transaction.CreatedAt
            };
        }


  

        public async Task<IEnumerable<InventoryTransactionResponse>>GetByProductAndWarehouseAsync(int productId,int warehouseId)
        {
    
            var transactions =
                await _transactionRepository.GetByWarehouseandproductIdAsync(warehouseId,productId);

            return transactions.Select(transaction =>
                new InventoryTransactionResponse
                {
                    Id = transaction.Id,

                    ProductId = transaction.ProductId,
                    ProductName = transaction.Product.Name,

                    WarehouseId = transaction.WarehouseId,
                    WarehouseName = transaction.Warehouse.Name,

                    Quantity = transaction.Quantity,

                    Type = transaction.Type,

                    Reference = transaction.ReferenceId,

                    CreatedAt = transaction.CreatedAt
                });
        }


        public async Task<IEnumerable<InventoryTransactionResponse>>GetByProductIdAsync(int productId)
        {
            var transactions =await _transactionRepository.GetByProductIdAsync(productId);

            return transactions.Select(transaction =>
                new InventoryTransactionResponse
                {
                    Id = transaction.Id,

                    ProductId = transaction.ProductId,
                    ProductName = transaction.Product.Name,

                    WarehouseId = transaction.WarehouseId,
                    WarehouseName = transaction.Warehouse.Name,

                    Quantity = transaction.Quantity,

                    Type = transaction.Type,

                    Reference = transaction.ReferenceId,

                    CreatedAt = transaction.CreatedAt
                });
        }

        public async Task<IEnumerable<InventoryTransactionResponse>> GetByWarehouseIdAsync(int warehouseId)
        {
            var transactions =
                await _transactionRepository
                    .GetByWarehouseIdAsync(warehouseId);

            return transactions.Select(transaction =>
                new InventoryTransactionResponse
                {
                    Id = transaction.Id,

                    ProductId = transaction.ProductId,
                    ProductName = transaction.Product.Name,

                    WarehouseId = transaction.WarehouseId,
                    WarehouseName = transaction.Warehouse.Name,

                    Quantity = transaction.Quantity,

                    Type = transaction.Type,

                    Reference = transaction.ReferenceId,

                    CreatedAt = transaction.CreatedAt
                });
        }

        public async Task<IEnumerable<InventoryTransactionResponse>>GetByTypeAsync(InventoryTransactionType type)
        {
            var transactions =await _transactionRepository.GetByTypeAsync(type);

            return transactions.Select(transaction =>
                new InventoryTransactionResponse
                {
                    Id = transaction.Id,

                    ProductId = transaction.ProductId,
                    ProductName = transaction.Product.Name,

                    WarehouseId = transaction.WarehouseId,
                    WarehouseName = transaction.Warehouse.Name,

                    Quantity = transaction.Quantity,

                    Type = transaction.Type,

                    Reference = transaction.ReferenceId,

                    CreatedAt = transaction.CreatedAt
                });
        }

        public async Task<InventoryTransactionResponse>ProcessTransactionAsync(CreateInventoryTransactionRequest request,int userId)
        {
            if (request.Quantity <= 0)
                throw new Exception( "Quantity must be greater than zero.");

            var product =await _productRepository.GetByIdAsync(request.ProductId);

            if (product == null)
                throw new Exception("Product does not exist.");

            var warehouse =await _warehouseRepository.GetByIdAsync(request.WarehouseId);

            if (warehouse == null)
                throw new Exception(
                    "Warehouse does not exist.");

            if (!warehouse.IsActive)
                throw new Exception(
                    "Warehouse is inactive.");


            var inventory =await _inventoryRepository.GetByProductAndWarehouseAsync(request.ProductId,request.WarehouseId);

            if (inventory == null)
                throw new Exception("Inventory does not exist for this product and warehouse.");

            InventoryTransaction? transaction = null;

            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                int transactionQuantity;

                switch (request.Type)
                {
                    case InventoryTransactionType.StockIn:

                        inventory.OnHandQuantity +=request.Quantity;

                        transactionQuantity =request.Quantity;

                        break;

                    case InventoryTransactionType.Sale:

                        var availableForSale =inventory.OnHandQuantity -inventory.ReservedQuantity;

                        if (request.Quantity >availableForSale)
                        {
                            throw new Exception("Insufficient available stock.");
                        }

                        inventory.OnHandQuantity -= request.Quantity;

                        transactionQuantity =-request.Quantity;

                        break;

                    case InventoryTransactionType.Return:

                        inventory.OnHandQuantity +=request.Quantity;

                        transactionQuantity =request.Quantity;

                        break;


                    case InventoryTransactionType.TransferIn:

                        inventory.OnHandQuantity +=request.Quantity;

                        transactionQuantity =request.Quantity;

                        break;

                    case InventoryTransactionType.TransferOut:

                        var availableForTransfer =inventory.OnHandQuantity -inventory.ReservedQuantity;

                        if (request.Quantity >availableForTransfer)
                        {
                            throw new Exception("Insufficient available stock.");
                        }

                        inventory.OnHandQuantity -=request.Quantity;

                        transactionQuantity =-request.Quantity;

                        break;

                    // Reservation and ReservationReleased
                    // are NOT handled here.
                  

                    default:

                        throw new Exception("This transaction type cannot be processed here.");
                }


                inventory.UpdatedAt =DateTime.UtcNow;

                _inventoryRepository.Update(inventory);


                transaction =
                    new InventoryTransaction
                    {
                        ProductId =
                            request.ProductId,

                        WarehouseId =
                            request.WarehouseId,

                        Quantity =
                            transactionQuantity,

                        Type =
                            request.Type,

                        ReferenceId =
                            request.Reference,

                        CreatedAt =
                            DateTime.UtcNow,

                        CreatedByUserId=userId

                    };


                await _transactionRepository.AddAsync(transaction);

                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                throw new Exception(
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

                      OnHandQuantity =inventory.OnHandQuantity,

                      ReservedQuantity =inventory.ReservedQuantity,

                      AvailableQuantity =inventory.OnHandQuantity -inventory.ReservedQuantity,

                      Reason =request.Type.ToString()
                  });
            //Low Stock
            var availableQuantity =inventory.OnHandQuantity -inventory.ReservedQuantity;

            if (availableQuantity <= inventory.ReorderLevel)
            {
                await _hubContext.Clients.All.SendAsync(
                    "LowStockAlert",
                    new
                    {
                        InventoryId = inventory.Id,

                        ProductId = inventory.ProductId,
                        ProductName = product.Name,

                        WarehouseId = inventory.WarehouseId,
                        WarehouseName = warehouse.Name,

                        AvailableQuantity = availableQuantity,

                        ReorderLevel = inventory.ReorderLevel
                    });
            }

            return new InventoryTransactionResponse
            {
                Id = transaction.Id,

                ProductId =
                       transaction.ProductId,

                ProductName =
                       product.Name,

                WarehouseId =
                       transaction.WarehouseId,

                WarehouseName =
                       warehouse.Name,

                Quantity =
                       transaction.Quantity,

                Type =
                       transaction.Type,

                Reference =
                       transaction.ReferenceId,

                CreatedAt =
                       transaction.CreatedAt
            };

        }
    }
}