using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request.Name);
            return Ok(category);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}

public record CreateCategoryRequest(string Name);
