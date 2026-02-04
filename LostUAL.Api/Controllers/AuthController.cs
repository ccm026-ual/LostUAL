using LostUAL.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public sealed record RegisterRequest(string Email, string Password);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, string UserId, string Email);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        await _userManager.AddToRoleAsync(user, "User");
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return Unauthorized();

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok) return Unauthorized();

        var (token, expiresAtUtc) = await CreateJwtAsync(user);
        return Ok(new AuthResponse(token, expiresAtUtc, user.Id, user.Email!));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email)
        });
    }

    private async Task<(string token, DateTime expiresAtUtc)> CreateJwtAsync(ApplicationUser user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var expiresMinutes = int.Parse(jwt["ExpiresMinutes"]!);

        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
    };

     
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

}
