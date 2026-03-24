namespace SmartTourApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Poi> Pois { get; set; } = new List<Poi>();
}
