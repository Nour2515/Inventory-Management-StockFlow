using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Orders;
using StockFlow.Interfaces;
using StockFlow.Services;
using System.Security.Claims;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderServices _orderServices;
    public OrdersController(IOrderServices orderServices)
    {
        _orderServices = orderServices;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        var order = await _orderServices.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> GetAllOrders() { 
    var orders = await _orderServices.GetAllAsync();
        return Ok(orders);
    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id) {
        var order = await _orderServices.GetByIdAsync(id);
        return Ok(order);
    }
    [HttpGet("My_Orders")]
    [Authorize(Roles="Customer")]
    public async Task<IActionResult> GetMyOrders() {
        var user_Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(user_Id)) return Unauthorized();
        if (!int.TryParse(user_Id, out var userId)) return Unauthorized();
        var orders=await _orderServices.GetMyOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request) 
    { 
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        var user_Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(user_Id)) return Unauthorized();
        if (!int.TryParse(user_Id, out var userId)) return Unauthorized();
        var order = await _orderServices.UpdateAsync(id, request,userId);
        return Ok(order);
    }
}
