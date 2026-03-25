using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;
using System.Security.Claims;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/poi-requests")]
public class PoiRequestsController : ControllerBase
{
    private readonly IPoiRequestService _requestService;
    public PoiRequestsController(IPoiRequestService requestService) => _requestService = requestService;

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _requestService.GetRequestCountsAsync(isAdmin ? null : userId));
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] string? status = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (User.IsInRole("Admin"))
            return Ok(await _requestService.GetAllPagedAsync(status, pageNumber, pageSize));
        return userId.HasValue
            ? Ok(await _requestService.GetByUserPagedAsync(userId.Value, status, pageNumber, pageSize))
            : Unauthorized();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var request = await _requestService.GetByIdAsync(id);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] PoiRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        var created = await _requestService.SubmitRequestAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] AdminActionDto dto)
    {
        var adminId = GetCurrentUserId();
        if (!adminId.HasValue) return Unauthorized();
        return await _requestService.ApproveAsync(id, adminId.Value, dto.Note) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] AdminActionDto dto)
    {
        var adminId = GetCurrentUserId();
        if (!adminId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Note)) return BadRequest("Vui lòng nhập lý do từ chối.");
        return await _requestService.RejectAsync(id, adminId.Value, dto.Note!) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        return await _requestService.DeleteAsync(id, userId.Value, User.IsInRole("Admin")) ? NoContent() : NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PoiRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        return await _requestService.UpdateRequestAsync(id, request.RequestData, userId.Value)
            ? NoContent()
            : NotFound("Yêu cầu không tìm thấy hoặc đã được duyệt.");
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class AdminActionDto { public string? Note { get; set; } }
