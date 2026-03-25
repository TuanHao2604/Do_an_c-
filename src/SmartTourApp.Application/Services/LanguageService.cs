using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class LanguageService : ILanguageService
{
    private readonly IAppDbContext _db;
    public LanguageService(IAppDbContext db) => _db = db;

    public async Task<List<Language>> GetLanguagesAsync()
        => await _db.Languages.OrderByDescending(l => l.IsDefault).ThenBy(l => l.Name).ToListAsync();

    public async Task<Language?> GetByCodeAsync(string code)
        => await _db.Languages.FirstOrDefaultAsync(l => l.Code == code);

    public async Task<Language> CreateOrUpdateAsync(Language language)
    {
        var existing = await _db.Languages.FirstOrDefaultAsync(l => l.Code == language.Code);
        if (existing is not null)
        {
            existing.Name = language.Name;
            existing.IsDefault = language.IsDefault;
        }
        else
        {
            _db.Languages.Add(language);
        }
        await _db.SaveChangesAsync();
        return existing ?? language;
    }

    public async Task<bool> ToggleActiveAsync(string code, bool isActive)
    {
        var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Code == code);
        if (lang is null || lang.IsDefault) return false;
        // Soft delete not supported on Language (no IsActive), just remove
        if (!isActive)
        {
            _db.Languages.Remove(lang);
            await _db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> SetDefaultAsync(string code)
    {
        var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Code == code);
        if (lang is null) return false;

        var all = await _db.Languages.ToListAsync();
        foreach (var l in all) l.IsDefault = false;
        lang.IsDefault = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
