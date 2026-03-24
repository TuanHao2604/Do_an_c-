using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiGuideController : ControllerBase
{
    private readonly IAiGuideService _aiGuideService;

    public AiGuideController(IAiGuideService aiGuideService)
    {
        _aiGuideService = aiGuideService;
    }

    /// <summary>
    /// ⭐ AI Virtual Guide: Get AI-generated description for a POI
    /// </summary>
    [HttpPost("guide")]
    public async Task<IActionResult> GetGuide([FromBody] AiGuideRequest request)
    {
        var description = await _aiGuideService.GetGuideDescriptionAsync(
            request.PoiId,
            request.LanguageCode ?? "vi");

        return Ok(new { description });
    }
}

public record AiGuideRequest(Guid PoiId, string? LanguageCode);
