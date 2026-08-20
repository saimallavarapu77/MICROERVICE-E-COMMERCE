namespace RestaurantService.API.Models;

public class Restaurant
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsOpen { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MenuCategory> Categories { get; set; }
        = new List<MenuCategory>();
}