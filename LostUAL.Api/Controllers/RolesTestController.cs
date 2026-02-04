using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/test")]
public class RolesTestController : ControllerBase
{
    [HttpGet("whoami")]
    [Authorize]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        });
    }

    [HttpGet("user")]
    [Authorize(Roles = "User,Moderator,Admin")]
    public IActionResult UserArea() => Ok("OK: User/Moderator/Admin");

    [HttpGet("moderator")]
    [Authorize(Roles = "Moderator,Admin")]
    public IActionResult ModeratorArea() => Ok("OK: Moderator/Admin");

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminArea() => Ok("OK: Admin");
}
