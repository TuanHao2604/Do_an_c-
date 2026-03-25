using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class TourService : ITourService
{
    private readonly IAppDbContext _db;
    public TourService(IAppDbContext db) => _db = db;

    public async Task<List<Tour>> GetAllAsync()
        => await _db.Tours.Where(t => t.IsActive).OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<Tour?> GetByIdAsync(Guid id)
        => await _db.Tours.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tour> CreateAsync(Tour tour)
    {
        tour.Id = Guid.NewGuid();
        tour.CreatedAt = DateTime.UtcNow;
        tour.UpdatedAt = DateTime.UtcNow;
        _db.Tours.Add(tour);
        await _db.SaveChangesAsync();
        return tour;
    }

    public async Task<Tour?> UpdateAsync(Guid id, Tour tour)
    {
        var existing = await _db.Tours.FirstOrDefaultAsync(t => t.Id == id);
        if (existing is null) return null;

        existing.Title = tour.Title;
        existing.Description = tour.Description;
        existing.DurationMinutes = tour.DurationMinutes;
        existing.PoiIds = tour.PoiIds;
        existing.IsActive = tour.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _db.Tours.FirstOrDefaultAsync(t => t.Id == id);
        if (existing is null) return false;
        existing.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
