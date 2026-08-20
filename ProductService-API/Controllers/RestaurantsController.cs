using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data;
using RestaurantService.API.Models;
using RestaurantService.API.Services;

namespace RestaurantService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{

    private readonly RestaurantDbContext _context;
    private readonly RedisCacheService _cache;

    public RestaurantsController(
        RestaurantDbContext context,
        RedisCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRestaurant(int id)
    {
        var cacheKey = $"restaurant:{id}";

        var cachedRestaurant =
            await _cache.GetAsync<Restaurant>(cacheKey);

        if (cachedRestaurant != null)
        {
            Console.WriteLine("REDIS CACHE HIT");

            return Ok(cachedRestaurant);
        }

        Console.WriteLine("REDIS CACHE MISS");

        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
            return NotFound();

        await _cache.SetAsync(
            cacheKey,
            restaurant,
            TimeSpan.FromMinutes(10));

        return Ok(restaurant);
    }
    [HttpGet("{id}/menu")]
    public async Task<IActionResult> GetMenu(int id)
    {
        var cacheKey = $"restaurant:{id}:menu";

        var cachedMenu =
            await _cache.GetAsync<object>(cacheKey);

        if (cachedMenu != null)
        {
            Console.WriteLine("MENU CACHE HIT");
            return Ok(cachedMenu);
        }

        Console.WriteLine("MENU CACHE MISS");

        var restaurantExists = await _context.Restaurants
            .AsNoTracking()
            .AnyAsync(r => r.Id == id);

        if (!restaurantExists)
            return NotFound("Restaurant not found.");

        var menu = await _context.MenuCategories
            .AsNoTracking()
            .Where(c => c.RestaurantId == id)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.DisplayOrder,
                Items = c.Items
                    .Where(i => i.IsAvailable)
                    .OrderBy(i => i.Name)
                    .Select(i => new
                    {
                        i.Id,
                        i.Name,
                        i.Description,
                        i.Price,
                        i.IsAvailable
                    })
                    .ToList()
            })
            .ToListAsync();

        await _cache.SetAsync(
            cacheKey,
            menu,
            TimeSpan.FromMinutes(10));

        return Ok(menu);
    }
    [HttpPut("menu/items/{itemId}")]
    public async Task<IActionResult> UpdateMenuItem(
    int itemId,
    decimal price,
    bool isAvailable)
    {
        var item = await _context.MenuItems
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            return NotFound("Menu item not found.");

        item.Price = price;
        item.IsAvailable = isAvailable;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _cache.RemoveAsync(
            $"restaurant:{item.RestaurantId}:menu");

        return Ok(item);
    }
}
