using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(string name);
    Task<CategoryDto> UpdateAsync(Guid id, string name);
    Task DeleteAsync(Guid id);
}

public record CategoryDto(Guid Id, string Name, int PoiCount);
