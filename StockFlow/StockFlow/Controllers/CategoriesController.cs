using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.DTOs.Catagory;
using StockFlow.Interfaces;
using System.Security.Claims;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) {
        _categoryService = categoryService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("Add")]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var category = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles ="Admin")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var category = await _categoryService.UpdateAsync(id, request);
        return Ok(category);
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
    }

