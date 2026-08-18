using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.API.Constants;
using UserService.API.Data;
using UserService.API.DTOs;
using UserService.API.Models;
using UserService.API.Services;
using UserService_API.Services;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly PasswordService _passwordService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(
     AppDbContext context,
     TokenService tokenService,
     PasswordService passwordService,
     RefreshTokenService refreshTokenService,
     IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        var usernameExists = await _context.Users
            .AnyAsync(x => x.Username == request.Username);

        if (usernameExists)
            return Conflict("Username already exists.");

        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
            return Conflict("Email already exists.");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordService.HashPassword(
                request.Password),
            Role = Roles.Customer,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User registered successfully.",
            userId = user.Id
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Username == request.Username);

        if (user == null)
            return Unauthorized("Invalid credentials.");

        var passwordValid =
            _passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
            return Unauthorized("Invalid credentials.");

        user.LastLoginAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var (token, expiresAt) =
            _tokenService.CreateToken(user);
        var refreshToken =
    _refreshTokenService.GenerateToken();

        var refreshTokenHash =
            _refreshTokenService.HashToken(refreshToken);

        var refreshTokenDays =
            Convert.ToDouble(
                _configuration["Jwt:RefreshTokenDurationInDays"] ?? "7");

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync();

        return Ok(new RefreshTokenResponse
        {
            AccessToken = token,
            AccessTokenExpiresAt = expiresAt,

            RefreshToken = refreshToken,
            RefreshTokenExpiresAt =
        refreshTokenEntity.ExpiresAt
        });
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Refresh token is required.");

        var tokenHash =
            _refreshTokenService.HashToken(
                request.RefreshToken);

        var refreshToken =
            await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash);

        if (refreshToken == null)
            return Unauthorized("Invalid refresh token.");

        if (refreshToken.IsRevoked)
            return Unauthorized("Refresh token has been revoked.");

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized("Refresh token has expired.");

        var user = refreshToken.User;

        // Revoke old refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Generate new access token
        var (accessToken, accessTokenExpiresAt) =
            _tokenService.CreateToken(user);

        // Generate new refresh token
        var newRefreshToken =
            _refreshTokenService.GenerateToken();

        var newRefreshTokenHash =
            _refreshTokenService.HashToken(
                newRefreshToken);

        var refreshTokenDays =
            Convert.ToDouble(
                _configuration["Jwt:RefreshTokenDurationInDays"]
                ?? "7");

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(
                refreshTokenDays)
        };

        _context.RefreshTokens.Add(
            newRefreshTokenEntity);

        await _context.SaveChangesAsync();

        return Ok(new RefreshTokenResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,

            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt =
                newRefreshTokenEntity.ExpiresAt
        });
    }
}