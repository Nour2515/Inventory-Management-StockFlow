using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Catagory;
using StockFlow.DTOs.warehouse;
using StockFlow.Interfaces;
using StockFlow.Models;
using StockFlow.Services;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService) { 
      _warehouseService = warehouseService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllWarehouses() {
        var warehouses = await _warehouseService.GetAllAsync();
        return Ok(warehouses);
    }
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetOneWarehouses(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        return Ok(warehouse);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateWarehouse(CreateWarehouseRequest request) {
     var warehouse=await _warehouseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetOneWarehouses), new { id = warehouse.Id }, warehouse);
    }
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,WarehouseManager")]
    public async Task<IActionResult> UpdateWarehouse(int id, UpdateWarehouseRequest request)
    {
        var warehouse = await _warehouseService.UpdateAsync(id, request);
        return Ok(warehouse);
    }   


}
