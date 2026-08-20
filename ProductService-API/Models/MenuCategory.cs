namespace RestaurantService.API.Models;

public class MenuCategory
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<MenuItem> Items { get; set; }
        = new List<MenuItem>();
}