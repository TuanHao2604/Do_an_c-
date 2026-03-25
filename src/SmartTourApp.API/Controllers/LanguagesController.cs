using Microsoft.AspNetCore.Mvc;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageService _languageService;
    public LanguagesController(ILanguageService languageService) => _languageService = languageService;

    [HttpGet]
    public async Task<IActionResult> GetLanguages()
        => Ok(await _languageService.GetLanguagesAsync());

    [HttpGet("{code}")]
    public async Task<IActionResult> GetLanguage(string code)
    {
        var lang = await _languageService.GetByCodeAsync(code);
        return lang is null ? NotFound() : Ok(lang);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] SmartTourApp.Domain.Entities.Language language)
        => Ok(await _languageService.CreateOrUpdateAsync(language));

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
        => await _languageService.ToggleActiveAsync(code, false) ? NoContent() : BadRequest("Không thể xóa ngôn ngữ mặc định.");

    [HttpPatch("{code}/set-default")]
    public async Task<IActionResult> SetDefault(string code)
        => await _languageService.SetDefaultAsync(code) ? NoContent() : NotFound();
}
