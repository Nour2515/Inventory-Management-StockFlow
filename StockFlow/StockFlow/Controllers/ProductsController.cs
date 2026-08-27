using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Product;
using StockFlow.Interfaces;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetoneProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Add")]
    public async Task<IActionResult> AddProduct (CreateProductRequest request){
    if(!ModelState.IsValid)
    {
        return BadRequest(ModelState);

    }
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetoneProduct),
            new { id = product.Id },
            product);
    }
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var product = await _productService.UpdateAsync(id, request);
        return Ok(product);
    }
    [HttpPatch("Deactivate/{id:int}")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> DeactivateProduct(int id)
    {
        await _productService.DeactivateAsync(id);
        return NoContent();
    }
}
