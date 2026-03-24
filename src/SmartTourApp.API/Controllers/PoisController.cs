using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Application.Queries;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoisController : ControllerBase
{
    private readonly IPoiService _poiService;
    private readonly IMediator _mediator;

    public PoisController(IPoiService poiService, IMediator mediator)
    {
        _poiService = poiService;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all POIs
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pois = await _poiService.GetAllAsync();
        return Ok(pois);
    }

    /// <summary>
    /// Get POI by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var poi = await _poiService.GetByIdAsync(id);
        if (poi is null) return NotFound();
        return Ok(poi);
    }

    /// <summary>
    /// ⭐ Core Feature: Find POIs near a location (PostGIS + Redis cache)
    /// Uses CQRS + MediatR
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 5)
    {
        var query = new GetPoisNearLocationQuery(lat, lng, radiusKm);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new POI
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreatePoiRequest request)
    {
        var poi = await _poiService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
    }

    /// <summary>
    /// Update an existing POI
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePoiRequest request)
    {
        try
        {
            var poi = await _poiService.UpdateAsync(id, request);
            return Ok(poi);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a POI
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _poiService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
