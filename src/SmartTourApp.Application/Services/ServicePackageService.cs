using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class ServicePackageService : IServicePackageService
{
    private readonly IAppDbContext _db;
    public ServicePackageService(IAppDbContext db) => _db = db;

    public async Task<List<ServicePackage>> GetAllActiveAsync(decimal? minPrice = null)
    {
        var query = _db.ServicePackages.AsQueryable();
        if (minPrice.HasValue)
            query = query.Where(sp => sp.Price >= minPrice.Value);
        return await query.OrderBy(sp => sp.Price).ToListAsync();
    }

    public async Task<ServicePackage?> GetByIdAsync(Guid id)
        => await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Id == id);

    public async Task<ServicePackage> CreateAsync(ServicePackage package)
    {
        package.Id = Guid.NewGuid();
        _db.ServicePackages.Add(package);
        await _db.SaveChangesAsync();
        return package;
    }

    public async Task<ServicePackage?> UpdateAsync(Guid id, ServicePackage package)
    {
        var existing = await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Id == id);
        if (existing is null) return null;

        existing.Code = package.Code;
        existing.Name = package.Name;
        existing.Description = package.Description;
        existing.Price = package.Price;
        existing.DurationDays = package.DurationDays;
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Id == id);
        if (existing is null) return false;
        _db.ServicePackages.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
