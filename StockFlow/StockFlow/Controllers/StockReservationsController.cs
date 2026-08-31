using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Reservation;
using StockFlow.Interfaces;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockReservationsController : ControllerBase
{
    private readonly IStockReservationService _stockReservationService;

    public StockReservationsController(IStockReservationService stockReservationService) {
        _stockReservationService = stockReservationService;
    
    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> GetById(int id) 
    {
        var reservation = await _stockReservationService.GetByIdAsync(id);
            if (reservation == null) 
            return NotFound(); 
        return Ok(reservation);
    }
    [HttpGet("order/{orderId:int}")]
    [Authorize(Roles = "Admin,InventoryManager")] 
    public async Task<IActionResult> GetByOrderId(int orderId) 
    { var reservations = await _stockReservationService.GetByOrderIdAsync(orderId);
        return Ok(reservations);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> CreateResevation([FromBody] CreateReservationRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var reservation=await _stockReservationService.CreateAsync(request);
        return Ok(reservation);
    }
    [HttpPut("{id:int}/release")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Release(int id)
    {
        await _stockReservationService.
            ReleaseAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Admin,InventoryManager")] 
    public async Task<IActionResult> Cancel(int id) { 
        await _stockReservationService.CancelAsync(id); 
        return NoContent();
    }


}
