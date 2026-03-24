using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface IAppDbContext
{
    DbSet<Language> Languages { get; }
    DbSet<Category> Categories { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<ServicePackage> ServicePackages { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    DbSet<Poi> Pois { get; }
    DbSet<PoiContent> PoiContents { get; }
    DbSet<PoiOperatingHours> PoiOperatingHours { get; }
    DbSet<PoiImage> PoiImages { get; }
    DbSet<AudioFile> AudioFiles { get; }
    DbSet<PoiManager> PoiManagers { get; }
    DbSet<Device> Devices { get; }
    DbSet<VisitLog> VisitLogs { get; }
    DbSet<LocationLog> LocationLogs { get; }
    DbSet<HeatmapCell> HeatmapCells { get; }
    DbSet<UserFavorite> UserFavorites { get; }
    DbSet<PoiReview> PoiReviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
