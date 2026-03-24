using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _db;

    public CategoryService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Pois.Count))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        return await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Pois.Count))
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(string name)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, 0);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, string name)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        category.Name = name;
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
