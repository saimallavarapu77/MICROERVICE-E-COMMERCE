using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Constants;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        return Ok(new
        {
            message = "Welcome Admin",
            role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role
            )?.Value
        });
    }
}