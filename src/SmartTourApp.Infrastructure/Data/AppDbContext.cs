using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ──
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<PoiContent> PoiContents => Set<PoiContent>();
    public DbSet<PoiOperatingHours> PoiOperatingHours => Set<PoiOperatingHours>();
    public DbSet<PoiImage> PoiImages => Set<PoiImage>();
    public DbSet<AudioFile> AudioFiles => Set<AudioFile>();
    public DbSet<PoiManager> PoiManagers => Set<PoiManager>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<VisitLog> VisitLogs => Set<VisitLog>();
    public DbSet<LocationLog> LocationLogs => Set<LocationLog>();
    public DbSet<HeatmapCell> HeatmapCells => Set<HeatmapCell>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
    public DbSet<PoiReview> PoiReviews => Set<PoiReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS
        modelBuilder.HasPostgresExtension("postgis");

        // ── Language ──
        modelBuilder.Entity<Language>(e =>
        {
            e.ToTable("languages");
            e.HasKey(l => l.Code);
            e.Property(l => l.Code).HasMaxLength(10);
            e.Property(l => l.Name).IsRequired().HasMaxLength(100);
        });

        // ── Category ──
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
        });

        // ── Role ──
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(r => r.Name).IsUnique();
        });

        // ── User ──
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        // ── ServicePackage ──
        modelBuilder.Entity<ServicePackage>(e =>
        {
            e.ToTable("service_packages");
            e.HasKey(sp => sp.Id);
            e.Property(sp => sp.Code).IsRequired().HasMaxLength(50);
            e.Property(sp => sp.Name).IsRequired().HasMaxLength(200);
            e.Property(sp => sp.Price).HasColumnType("decimal(18,2)");
            e.HasIndex(sp => sp.Code).IsUnique();
        });

        // ── UserSubscription ──
        modelBuilder.Entity<UserSubscription>(e =>
        {
            e.ToTable("user_subscriptions");
            e.HasKey(us => us.Id);
            e.Property(us => us.Status).IsRequired().HasMaxLength(20);
            e.HasOne(us => us.User).WithMany(u => u.Subscriptions).HasForeignKey(us => us.UserId);
            e.HasOne(us => us.Package).WithMany(sp => sp.Subscriptions).HasForeignKey(us => us.PackageId);
        });

        // ── Poi (PostGIS) ──
        modelBuilder.Entity<Poi>(e =>
        {
            e.ToTable("pois");
            e.HasKey(p => p.Id);
            e.Property(p => p.Location).HasColumnType("geometry(Point, 4326)");
            e.Property(p => p.QrValue).HasMaxLength(500);
            e.HasOne(p => p.Category).WithMany(c => c.Pois).HasForeignKey(p => p.CategoryId);
            e.HasIndex(p => p.Location).HasMethod("GIST"); // Spatial index
        });

        // ── PoiContent ──
        modelBuilder.Entity<PoiContent>(e =>
        {
            e.ToTable("poi_contents");
            e.HasKey(pc => pc.Id);
            e.Property(pc => pc.LanguageCode).IsRequired().HasMaxLength(10);
            e.Property(pc => pc.Name).IsRequired().HasMaxLength(300);
            e.Property(pc => pc.Address).HasMaxLength(500);
            e.HasOne(pc => pc.Poi).WithMany(p => p.Contents).HasForeignKey(pc => pc.PoiId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pc => pc.Language).WithMany(l => l.PoiContents).HasForeignKey(pc => pc.LanguageCode);
            e.HasIndex(pc => new { pc.PoiId, pc.LanguageCode }).IsUnique();
        });

        // ── PoiOperatingHours ──
        modelBuilder.Entity<PoiOperatingHours>(e =>
        {
            e.ToTable("poi_operating_hours");
            e.HasKey(oh => oh.Id);
            e.HasOne(oh => oh.Poi).WithMany(p => p.OperatingHours).HasForeignKey(oh => oh.PoiId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(oh => new { oh.PoiId, oh.DayOfWeek }).IsUnique();
        });

        // ── PoiImage ──
        modelBuilder.Entity<PoiImage>(e =>
        {
            e.ToTable("poi_images");
            e.HasKey(pi => pi.Id);
            e.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(1000);
            e.HasOne(pi => pi.Poi).WithMany(p => p.Images).HasForeignKey(pi => pi.PoiId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── AudioFile ──
        modelBuilder.Entity<AudioFile>(e =>
        {
            e.ToTable("audio_files");
            e.HasKey(af => af.Id);
            e.Property(af => af.LanguageCode).IsRequired().HasMaxLength(10);
            e.Property(af => af.FileUrl).IsRequired().HasMaxLength(1000);
            e.HasOne(af => af.Poi).WithMany(p => p.AudioFiles).HasForeignKey(af => af.PoiId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(af => af.Language).WithMany(l => l.AudioFiles).HasForeignKey(af => af.LanguageCode);
        });

        // ── PoiManager (Composite PK) ──
        modelBuilder.Entity<PoiManager>(e =>
        {
            e.ToTable("poi_managers");
            e.HasKey(pm => new { pm.UserId, pm.PoiId });
            e.HasOne(pm => pm.User).WithMany(u => u.ManagedPois).HasForeignKey(pm => pm.UserId);
            e.HasOne(pm => pm.Poi).WithMany(p => p.Managers).HasForeignKey(pm => pm.PoiId);
        });

        // ── Device ──
        modelBuilder.Entity<Device>(e =>
        {
            e.ToTable("devices");
            e.HasKey(d => d.Id);
            e.Property(d => d.DeviceUuid).IsRequired().HasMaxLength(200);
            e.Property(d => d.Platform).IsRequired().HasMaxLength(50);
            e.Property(d => d.DeviceToken).HasMaxLength(500);
            e.Property(d => d.DeviceModel).HasMaxLength(200);
            e.HasIndex(d => d.DeviceUuid).IsUnique();
            e.HasOne(d => d.User).WithMany(u => u.Devices).HasForeignKey(d => d.UserId);
        });

        // ── VisitLog ──
        modelBuilder.Entity<VisitLog>(e =>
        {
            e.ToTable("visit_logs");
            e.HasKey(vl => vl.Id);
            e.Property(vl => vl.TriggerType).IsRequired().HasMaxLength(50);
            e.HasOne(vl => vl.Device).WithMany(d => d.VisitLogs).HasForeignKey(vl => vl.DeviceId);
            e.HasOne(vl => vl.Poi).WithMany(p => p.VisitLogs).HasForeignKey(vl => vl.PoiId);
            e.HasIndex(vl => vl.VisitedAt);
        });

        // ── LocationLog (PostGIS) ──
        modelBuilder.Entity<LocationLog>(e =>
        {
            e.ToTable("location_logs");
            e.HasKey(ll => ll.Id);
            e.Property(ll => ll.Location).HasColumnType("geometry(Point, 4326)");
            e.HasOne(ll => ll.Device).WithMany(d => d.LocationLogs).HasForeignKey(ll => ll.DeviceId);
            e.HasIndex(ll => ll.LoggedAt);
        });

        // ── HeatmapCell ──
        modelBuilder.Entity<HeatmapCell>(e =>
        {
            e.ToTable("heatmap_cells");
            e.HasKey(hc => hc.Id);
            e.HasIndex(hc => new { hc.GridLat, hc.GridLng, hc.HourBucket }).IsUnique();
        });

        // ── UserFavorite (Composite PK) ──
        modelBuilder.Entity<UserFavorite>(e =>
        {
            e.ToTable("user_favorites");
            e.HasKey(uf => new { uf.UserId, uf.PoiId });
            e.HasOne(uf => uf.User).WithMany(u => u.Favorites).HasForeignKey(uf => uf.UserId);
            e.HasOne(uf => uf.Poi).WithMany(p => p.Favorites).HasForeignKey(uf => uf.PoiId);
        });

        // ── PoiReview ──
        modelBuilder.Entity<PoiReview>(e =>
        {
            e.ToTable("poi_reviews");
            e.HasKey(pr => pr.Id);
            e.HasOne(pr => pr.User).WithMany(u => u.Reviews).HasForeignKey(pr => pr.UserId);
            e.HasOne(pr => pr.Poi).WithMany(p => p.Reviews).HasForeignKey(pr => pr.PoiId);
            e.HasIndex(pr => pr.CreatedAt);
        });

        // ── Seed Data ──
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Roles
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var managerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin" },
            new Role { Id = userRoleId, Name = "User" },
            new Role { Id = managerRoleId, Name = "Manager" }
        );

        // Languages
        modelBuilder.Entity<Language>().HasData(
            new Language { Code = "vi", Name = "Tiếng Việt", IsDefault = true },
            new Language { Code = "en", Name = "English", IsDefault = false }
        );

        // Admin user (password: Admin@123)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Admin",
                Email = "admin@smarttour.com",
                RoleId = adminRoleId,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );

        // Sample categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), Name = "Điểm tham quan" },
            new Category { Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"), Name = "Nhà hàng" },
            new Category { Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"), Name = "Khách sạn" },
            new Category { Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"), Name = "Bảo tàng" }
        );
    }
}
