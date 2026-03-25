using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IAppDbContext _db;
    public SubscriptionService(IAppDbContext db) => _db = db;

    public async Task<UserSubscription?> GetByUserIdAsync(Guid userId)
        => await _db.UserSubscriptions
            .Include(us => us.Package)
            .Where(us => us.UserId == userId && us.Status == "Active")
            .OrderByDescending(us => us.StartDate)
            .FirstOrDefaultAsync();

    public async Task<bool> SubscribeDefaultPackageAsync(Guid userId)
    {
        var defaultPkg = await _db.ServicePackages
            .Where(sp => sp.Price == 0)
            .FirstOrDefaultAsync();
        if (defaultPkg is null) return false;

        return await ActivateSubscriptionAsync(userId, defaultPkg.Id);
    }

    public async Task<bool> ActivateSubscriptionAsync(Guid userId, Guid packageId)
    {
        var package = await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Id == packageId);
        if (package is null) return false;

        // Deactivate current subscription
        var current = await _db.UserSubscriptions
            .Where(us => us.UserId == userId && us.Status == "Active")
            .ToListAsync();
        foreach (var sub in current) sub.Status = "Expired";

        _db.UserSubscriptions.Add(new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = packageId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
        });
        await _db.SaveChangesAsync();
        return true;
    }
}
