using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IUploadService _uploadService;
    public UploadsController(IUploadService uploadService) => _uploadService = uploadService;

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var url = await _uploadService.UploadImageAsync(stream, file.FileName, file.ContentType);
        return Ok(new { url });
    }

    [HttpPost("audio")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var url = await _uploadService.UploadAudioAsync(stream, file.FileName, file.ContentType);
        return Ok(new { url });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile([FromQuery] string url)
    {
        var result = await _uploadService.DeleteFileAsync(url);
        return result ? NoContent() : NotFound();
    }
}
