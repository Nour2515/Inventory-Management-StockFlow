using StockFlow.DTOs.warehouse;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Services
{
    public class WarehouseServices : IWarehouseService
    {
        public readonly IGenericRepository<Warehouse> _warehouserepo;

        public WarehouseServices(IGenericRepository<Warehouse> warehouserepo)
        {
            _warehouserepo = warehouserepo;
        }
        public async Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request)
        {
            var warehouse = new Warehouse
            {
                Name = request.name,
                Location = request.location,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await _warehouserepo.AddAsync(warehouse);
            await _warehouserepo.SaveChangesAsync();
            return new WarehouseResponse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                location = warehouse.Location
            };
        }

        public async Task<string> Deactivate(int id)
        {
            var warehouse = await _warehouserepo.GetByIdAsync(id);
            if(warehouse==null)
                throw new Exception("Warehouse not found.");
            warehouse.IsActive = false;
            _warehouserepo.Update(warehouse);
            await _warehouserepo.SaveChangesAsync();
            return "Warehouse deactivated successfully.";

        }

        public async Task<IEnumerable<WarehouseResponse>> GetAllAsync()
        {
            var warehouses = await _warehouserepo.GetAllAsync();
            return warehouses.Select(w => new WarehouseResponse
            {
                Id = w.Id,
                Name = w.Name,
                location = w.Location,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt
            });
        }

        public async Task<WarehouseResponse?> GetByIdAsync(int id)
        {
            var warehouse = await _warehouserepo.GetByIdAsync(id);
            if (warehouse == null) return null;

            return new WarehouseResponse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                location = warehouse.Location,
                IsActive = warehouse.IsActive,
                CreatedAt = warehouse.CreatedAt
            };
        }

        public async Task<WarehouseResponse> UpdateAsync(int id, UpdateWarehouseRequest request)
        {
            var warehouse = await _warehouserepo.GetByIdAsync(id);
            if (warehouse == null) 
                throw new InvalidOperationException("Warehouse not found");

            warehouse.Name = request.name;
            warehouse.Location = request.location;

            _warehouserepo.Update(warehouse);
            await _warehouserepo.SaveChangesAsync();
            return new WarehouseResponse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                location = warehouse.Location
            };
        }
        }
    }
