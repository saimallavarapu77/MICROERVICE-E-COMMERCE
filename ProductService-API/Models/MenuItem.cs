namespace RestaurantService.API.Models;

public class MenuItem
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Restaurant Restaurant { get; set; } = null!;

    public MenuCategory Category { get; set; } = null!;
}