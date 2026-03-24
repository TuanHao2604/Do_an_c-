using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService)
    {
        _syncService = syncService;
    }

    /// <summary>
    /// ⭐ Offline Sync: Get POIs changed since lastUpdated timestamp.
    /// Mobile app calls this periodically to sync data.
    /// </summary>
    [HttpGet("pois")]
    public async Task<IActionResult> GetChangedPois([FromQuery] DateTime? lastUpdated)
    {
        var since = lastUpdated ?? DateTime.MinValue;
        var result = await _syncService.GetChangedPoisAsync(since);
        return Ok(result);
    }
}
