using Microsoft.Extensions.Caching.Distributed;
using StockFlow.core;
using StockFlow.DTOs.Inventory;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Services
{
    public class InventoryServices : IinventoryServices
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly ICacheService _cacheService;


        public InventoryServices(IInventoryRepository inventoryRepository, IGenericRepository<Product> productRepository, IGenericRepository<Warehouse> warehouseRepository, ICacheService cacheService)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _cacheService = cacheService;
        }

        public async Task<InventoryResponse> CreateAsync(CreateInventoryRequest request)
        { 
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if(product==null)
                throw new Exception("product does not exist");
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
                throw new Exception("warehouse does not exist");
            if (!warehouse.IsActive) 
                throw new Exception("Warehouse is inactive.");
            var existingInventory = await _inventoryRepository.GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId);
            if(existingInventory!=null)
                throw new Exception("Inventory already exists for this product and warehouse.");
            if (request.onhandQuantity < 0)
                throw new Exception("quantity cannot be negative.");
            var inventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                OnHandQuantity = request.onhandQuantity,
                ReorderLevel = request.ReorderLevel,
                ReservedQuantity = 0,
                UpdatedAt = DateTime.UtcNow,
            };
            await _inventoryRepository.AddAsync(inventory);
            await _inventoryRepository.SaveChangesAsync();
            await _cacheService.InvalidateInventoryCacheAsync(inventory);
            return new InventoryResponse {
                Id = inventory.Id, 
                ProductId = inventory.ProductId, 
                ProductName = product.Name, 
                WarehouseId = inventory.WarehouseId, 
                WarehouseName = warehouse.Name,
                OnHandQuantity = inventory.OnHandQuantity, 
                ReservedQuantity = inventory.ReservedQuantity, 
                AvailableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity,
                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt 
            };
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<InventoryResponse>> GetAllAsync()
        {
            //using Redis
            var cacheKey = CacheKeys.InventoryAll;
            var cachedInventories = await _cacheService.GetAsync<List<InventoryResponse>>(cacheKey);
            if (cachedInventories != null)
            {
                return cachedInventories;
            }
            //Redis MISS
            var Inventories = await _inventoryRepository.GetAllAsync();
            var result= Inventories.Select(inventory => new InventoryResponse
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                OnHandQuantity = inventory.OnHandQuantity,
                ReservedQuantity = inventory.ReservedQuantity,
                AvailableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity,
                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt
            }).ToList();
        await _cacheService.SetAsync(cacheKey,result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<InventoryResponse?> GetByIdAsync(int id)
        {
            var cacheKey = CacheKeys.InventoryById(id);
            var cachedInventory = await _cacheService.GetAsync<InventoryResponse>(cacheKey);
            if (cachedInventory != null)
            {
                return cachedInventory;
            }

            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
                return null;

            var result = new InventoryResponse
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                OnHandQuantity = inventory.OnHandQuantity,
                ReservedQuantity = inventory.ReservedQuantity,
                AvailableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity,
                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt
            };
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<InventoryResponse?> UpdateAsync(int id, UpdateInventoryRequest request)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
                return null;
            if (request.ReorderLevel < 0)
                throw new Exception("Reorder level cannot be negative");
            inventory.ReorderLevel = request.ReorderLevel;
            inventory.UpdatedAt = DateTime.UtcNow;
            _inventoryRepository.Update(inventory);
            await _inventoryRepository.SaveChangesAsync();
            //cache Invalidation
            await _cacheService.InvalidateInventoryCacheAsync(inventory);
            return new InventoryResponse
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                OnHandQuantity = inventory.OnHandQuantity,
                ReservedQuantity = inventory.ReservedQuantity,
                AvailableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity,
                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt

            };
        }
      
        public async Task<List<InventoryResponse>> GetByWarehouseIdAsync(int warehouseId)
        {
            var cacheKey = CacheKeys.InventoryByWarehouse(warehouseId);
            var cachedInventories = await _cacheService.GetAsync<List<InventoryResponse>>(cacheKey);
            if (cachedInventories != null)
            {
                return cachedInventories;
            }

            var inventories = await _inventoryRepository.GetByWarehouseIdAsync(warehouseId);

            var result = inventories.Select(inventory => new InventoryResponse
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,
                OnHandQuantity = inventory.OnHandQuantity,
                ReservedQuantity = inventory.ReservedQuantity,
            
                AvailableQuantity =inventory.OnHandQuantity - inventory.ReservedQuantity,
            
                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt
            
            }).ToList();

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        public async Task<List<InventoryResponse>> GetByproductIdAsync(int ProductId)
        {
            var cacheKey = CacheKeys.InventoryByProduct(ProductId);
            var cachedInventories = await _cacheService.GetAsync<List<InventoryResponse>>(cacheKey);
            if (cachedInventories != null) { return cachedInventories; }
            var inventories = await _inventoryRepository.GetByProductIdAsync(ProductId);

            var result = inventories.Select(inventory => new InventoryResponse
            {
                Id = inventory.Id,

                ProductId=inventory.ProductId,

                ProductName = inventory.Product.Name,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse.Name,

                OnHandQuantity = inventory.OnHandQuantity,
                ReservedQuantity = inventory.ReservedQuantity,

                AvailableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity,

                ReorderLevel = inventory.ReorderLevel,
                UpdatedAt = inventory.UpdatedAt

            }).ToList();
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<StockAvailabilityResponse> CheckAvailabilityAsync(int productId, int warehouseId, int quantity) {
            if (quantity <= 0)
                throw new Exception("Quantity must be greater than zero");
            var inventory = await _inventoryRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
            if (inventory == null)
                throw new Exception("inventory not found");
            var availableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity;
            var IsAvailable = true;
            if (quantity > availableQuantity) { 
            IsAvailable = false;
            }
            return new StockAvailabilityResponse 
            {
             productId = inventory.ProductId,
             warehouseId = inventory.WarehouseId,
             requstedQuantity = quantity,
             availableQuantity = availableQuantity,
             isAvailable = IsAvailable 
            };

        }
     
        }

    }

    

