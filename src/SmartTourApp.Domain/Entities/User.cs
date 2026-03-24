namespace SmartTourApp.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Role Role { get; set; } = null!;
    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<PoiManager> ManagedPois { get; set; } = new List<PoiManager>();
    public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
    public ICollection<PoiReview> Reviews { get; set; } = new List<PoiReview>();
}
