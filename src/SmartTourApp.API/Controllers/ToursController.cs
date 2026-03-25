using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly ITourService _tourService;
    public ToursController(ITourService tourService) => _tourService = tourService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _tourService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tour = await _tourService.GetByIdAsync(id);
        return tour is null ? NotFound() : Ok(tour);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Tour tour)
    {
        var created = await _tourService.CreateAsync(tour);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Tour tour)
    {
        var updated = await _tourService.UpdateAsync(id, tour);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _tourService.DeleteAsync(id) ? NoContent() : NotFound();
}
