using Microsoft.AspNetCore.Mvc;
using UserService.API.Data;
using UserService.API.Models;
using UserService.API.Services;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User Registered Successfully");
    }

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Users
            .FirstOrDefault(x => x.Username == username && x.Password == password);

        if (user == null)
            return Unauthorized("Invalid Credentials");

        var token = _tokenService.CreateToken(user);

        return Ok(new { token });
    }
}
