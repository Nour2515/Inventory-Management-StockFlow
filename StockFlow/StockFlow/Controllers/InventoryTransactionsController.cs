using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Transaction;
using StockFlow.Interfaces;
using StockFlow.Models.Enums;
using StockFlow.Services;
using System.Security.Claims;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryTransactionsController : ControllerBase
{
    private readonly IinventoryTransactionservice _inventoryTransactionservice;

    public InventoryTransactionsController(IinventoryTransactionservice iinventoryTransactionservice) { 
    _inventoryTransactionservice = iinventoryTransactionservice;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllTransactions() { 
    var Transactions=await _inventoryTransactionservice.GetAllAsync();
        return Ok(Transactions);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var Transaction = await _inventoryTransactionservice.GetByIdAsync(id);
        return Ok(Transaction);
    }
    [HttpPost]
    public async Task<IActionResult> ProcessTransaction([FromBody] CreateInventoryTransactionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var transaction = await _inventoryTransactionservice.ProcessTransactionAsync(request, userId);


        return Ok(transaction);
    }
      [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var transactions =await _inventoryTransactionservice.GetByProductIdAsync(productId);

            return Ok(transactions);
        }

    [HttpGet("warehouse/{warehouseId:int}")]
    public async Task<IActionResult> GetByWarehouse(
            int warehouseId)
    {
        var transactions =await _inventoryTransactionservice.GetByWarehouseIdAsync(warehouseId);

        return Ok(transactions);
    }


    [HttpGet("product/{productId:int}/warehouse/{warehouseId:int}")]
    public async Task<IActionResult> GetByProductAndWarehouse(int productId,int warehouseId)
    {
        var transactions =await _inventoryTransactionservice.GetByProductAndWarehouseAsync(productId,warehouseId);

        return Ok(transactions);
    }
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(InventoryTransactionType type)
    {
        var transactions =await _inventoryTransactionservice.GetByTypeAsync(type);

        return Ok(transactions);
    }
}
