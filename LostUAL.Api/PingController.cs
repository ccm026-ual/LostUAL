using Microsoft.AspNetCore.Mvc;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new { status = "ok", time = DateTimeOffset.UtcNow });
}
