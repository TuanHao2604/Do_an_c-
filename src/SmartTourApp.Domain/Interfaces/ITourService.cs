using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface ITourService
{
    Task<List<Tour>> GetAllAsync();
    Task<Tour?> GetByIdAsync(Guid id);
    Task<Tour> CreateAsync(Tour tour);
    Task<Tour?> UpdateAsync(Guid id, Tour tour);
    Task<bool> DeleteAsync(Guid id);
}
