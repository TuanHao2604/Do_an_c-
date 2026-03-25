using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _packageService;
    public ServicePackagesController(IServicePackageService packageService) => _packageService = packageService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] decimal? minPrice = null)
        => Ok(await _packageService.GetAllActiveAsync(minPrice));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var pkg = await _packageService.GetByIdAsync(id);
        return pkg is null ? NotFound() : Ok(pkg);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServicePackage package)
    {
        var created = await _packageService.CreateAsync(package);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ServicePackage package)
    {
        var updated = await _packageService.UpdateAsync(id, package);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _packageService.DeleteAsync(id) ? NoContent() : NotFound();
}
