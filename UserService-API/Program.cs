using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using UserService.API.Data;
using UserService.API.Services;
using UserService_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Service API",
        Version = "v1"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var jwt = builder.Configuration.GetSection("Jwt");

var jwtKey = jwt["Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<RefreshTokenService>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "User Service API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();