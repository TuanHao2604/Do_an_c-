using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface ILanguageService
{
    Task<List<Language>> GetLanguagesAsync();
    Task<Language?> GetByCodeAsync(string code);
    Task<Language> CreateOrUpdateAsync(Language language);
    Task<bool> ToggleActiveAsync(string code, bool isActive);
    Task<bool> SetDefaultAsync(string code);
}
