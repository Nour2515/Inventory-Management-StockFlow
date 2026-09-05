using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Reservation;
using StockFlow.Interfaces;
using System.Security.Claims;

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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        var reservation=await _stockReservationService.CreateAsync(request, userId);
        return Ok(reservation);
    }
    [HttpPut("{id:int}/release")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> Release(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        await _stockReservationService.ReleaseAsync(id, userId);
        return NoContent();
    }

    [HttpPut("cancel/{id:int}")]
    [Authorize(Roles = "Admin,InventoryManager")] 
    public async Task<IActionResult> Cancel(int id) {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        await _stockReservationService.CancelAsync(id,userId); 
        return NoContent();
    }


}
