using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var paymentUrl = await _paymentService.CreatePaymentLinkAsync(request.UserId, request.PackageCode, request.Type ?? "New");
            return Ok(new { paymentUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] WebhookPayload payload)
    {
        await _paymentService.ProcessWebhookAsync(payload.TransactionId, payload.Status);
        return Ok(new { success = true });
    }

    [HttpGet("verify/{id}")]
    public async Task<IActionResult> Verify(Guid id)
    {
        var status = await _paymentService.GetPaymentStatusAsync(id);
        return status == "NotFound" ? NotFound() : Ok(new { status });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
        => Ok(await _paymentService.GetPaymentsByUserAsync(userId));
}

public class CreatePaymentRequest
{
    public Guid UserId { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string? Type { get; set; }
}

public class WebhookPayload
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
