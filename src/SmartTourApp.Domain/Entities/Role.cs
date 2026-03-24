namespace SmartTourApp.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
