namespace RestaurantService.API.Models;

public class OutboxEvent
{
    public long Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }
}