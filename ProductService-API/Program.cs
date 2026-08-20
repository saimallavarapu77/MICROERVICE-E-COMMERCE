using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data;
using RestaurantService.API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "RestaurantDb")));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var connectionString =
        configuration["Redis:ConnectionString"] ?? "localhost:6379";

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddScoped<RedisCacheService>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();