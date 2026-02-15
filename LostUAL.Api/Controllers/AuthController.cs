using LostUAL.Contracts.Auth;
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

        if (!IsAllowedEmail(email))
            return BadRequest("Solo se permiten correos @ual.es o @inlumine.ual.es");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description).ToList());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors.Select(e => e.Description).ToList());
        }

        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return Unauthorized();

        if (await _userManager.IsLockedOutAsync(user))
        {
            var end = await _userManager.GetLockoutEndDateAsync(user);
            var until = end?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "";
            return StatusCode(StatusCodes.Status423Locked,$"Usuario bloqueado por moderación hasta {until}");
        }

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok) return Unauthorized();

        var (token, expiresAtUtc) = await CreateJwtAsync(user);
        return Ok(new AuthResponse(token, expiresAtUtc, user.Id, user.Email!));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AccountProfileDto>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AccountProfileDto(
            UserId: user.Id,
            Email: user.Email ?? user.UserName ?? "",
            Roles: roles.ToList(),
            CreatedAtUtc: user.CreatedAtUtc
        ));
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

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok();
    }

    [HttpPost("change-email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        if (!IsAllowedEmail(req.NewEmail))
            return BadRequest("Solo se permiten correos @ual.es o @inlumine.ual.es");

        if (!await _userManager.CheckPasswordAsync(user, req.CurrentPassword))
            return BadRequest(new[] { "La contraseña actual no es válida." });

        var existing = await _userManager.FindByEmailAsync(req.NewEmail);
        if (existing is not null && existing.Id != user.Id)
            return BadRequest(new[] { "Ese email ya está en uso." });

        user.Email = req.NewEmail;
        user.UserName = req.NewEmail;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            return BadRequest(update.Errors.Select(e => e.Description));

        return Ok();
    }
    static bool IsAllowedEmail(string email)
    {
        email = email.Trim().ToLowerInvariant();
        return email.EndsWith("@ual.es") || email.EndsWith("@inlumine.ual.es");
    }
}


