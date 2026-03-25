using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenueService;
    public RevenueController(IRevenueService revenueService) => _revenueService = revenueService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        => Ok(await _revenueService.GetStatisticsAsync(startDate, endDate));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        => Ok(await _revenueService.GetPaymentsAsync(startDate, endDate, pageNumber, pageSize));
}
