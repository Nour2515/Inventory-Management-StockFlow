using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Inventory;
using StockFlow.Interfaces;
using StockFlow.Services;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IinventoryServices _inventoryServices;
    public InventoryController(IinventoryServices inventoryServices)
    {
        _inventoryServices = inventoryServices;
    }
    [HttpGet]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> GetAllInventory()
    {
        var inventory = await _inventoryServices.GetAllAsync();
        return Ok(inventory);
    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,InventoryManager")]

    public async Task<IActionResult> GetInventoryItem(int id)
    {
        var inventoryItem = await _inventoryServices.GetByIdAsync(id);
        if (inventoryItem == null)
            return NotFound();
        return Ok(inventoryItem);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,InventoryManager")]

    public async Task<IActionResult> AddInventory(CreateInventoryRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var Inventory = await _inventoryServices.CreateAsync(request);
        return Created();
    }
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> UpdateInventoy(int id, UpdateInventoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var Inventory = await _inventoryServices.UpdateAsync(id, request);
        return Ok(Inventory);

    }
    [HttpGet("availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] int productId,[FromQuery] int warehouseId,[FromQuery] int quantity)
    { 
        var result = await _inventoryServices.CheckAvailabilityAsync(productId, warehouseId, quantity);
        return Ok(result); 
    }
    [HttpGet("GetByProduct/{id}")]
    public async Task<IActionResult> GetByProduct(int id) { 
    var ProductInventory=await _inventoryServices.GetByproductIdAsync(id);
        return Ok(ProductInventory);
    }
    [HttpGet("GetByWarehouse/{id}")]
    public async Task<IActionResult> GetByWarehouse(int id)
    {
        var WarehouseInventory = await _inventoryServices.GetByWarehouseIdAsync(id);
        return Ok(WarehouseInventory);
    }


}
