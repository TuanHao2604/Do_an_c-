using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    public SubscriptionsController(ISubscriptionService subscriptionService) => _subscriptionService = subscriptionService;

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSubscription(Guid userId)
    {
        var sub = await _subscriptionService.GetByUserIdAsync(userId);
        return sub is null ? NotFound("Chưa có gói dịch vụ nào.") : Ok(sub);
    }

    [HttpPost("subscribe-default")]
    public async Task<IActionResult> SubscribeDefault([FromBody] SubscribeDefaultRequest request)
    {
        var result = await _subscriptionService.SubscribeDefaultPackageAsync(request.UserId);
        return result
            ? Ok(new { success = true, message = "Đã đăng ký gói mặc định" })
            : BadRequest(new { success = false, message = "Không thể đăng ký" });
    }
}

public class SubscribeDefaultRequest { public Guid UserId { get; set; } }
