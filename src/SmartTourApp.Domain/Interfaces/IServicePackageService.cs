using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface IServicePackageService
{
    Task<List<ServicePackage>> GetAllActiveAsync(decimal? minPrice = null);
    Task<ServicePackage?> GetByIdAsync(Guid id);
    Task<ServicePackage> CreateAsync(ServicePackage package);
    Task<ServicePackage?> UpdateAsync(Guid id, ServicePackage package);
    Task<bool> DeleteAsync(Guid id);
}
