using LostUAL.Contracts.Chat;
using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Posts;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly LostUALDbContext _db;
    public ClaimsController(LostUALDbContext db) => _db = db;

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var items = await _db.Claims
            .AsNoTracking()
            .Where(c => c.ClaimantUserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new MyClaimListItemDto(
                c.Id,
                c.PostId,
                c.Post!.Title,
                c.Post!.Type,
                c.Status,
                c.CreatedAtUtc,
                c.Conversation != null ? (int?)c.Conversation.Id : null
            ))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var items = await _db.Claims
            .AsNoTracking()
            .Where(c => c.Post!.CreatedByUserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new InboxClaimListItemDto(
                c.Id,
                c.PostId,
                c.Post!.Title,
                c.Post!.Type,
                c.Status,
                c.CreatedAtUtc,
                c.ClaimantUserId,
                _db.Users.Where(u => u.Id == c.ClaimantUserId).Select(u => u.Email).FirstOrDefault(),
                c.Conversation != null ? (int?)c.Conversation.Id : null
            ))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> Accept(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var claim = await _db.Claims
            .Include(c => c.Post)
            .Include(c => c.Conversation)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (claim is null)
            return NotFound();

        if (claim.Post!.CreatedByUserId != userId)
            return Forbid();

        if (claim.Status is not (ClaimStatus.Pending or ClaimStatus.Standby))
            return BadRequest("Solo se puede aceptar una claim en estado Pending o Standby.");

        if (claim.Post.Status != PostStatus.Open)
            return BadRequest("Solo se puede aceptar si el post está en estado Open.");

        var alreadyAccepted = await _db.Claims.AnyAsync(c =>
            c.PostId == claim.PostId &&
            c.Id != claim.Id &&
            c.Status == ClaimStatus.Accepted, ct);

        if (alreadyAccepted)
            return Conflict("Ya existe una claim aceptada para este post.");

        claim.Status = ClaimStatus.Accepted;

        claim.Post.Status = PostStatus.InClaim;

        var othersPending = await _db.Claims
            .Where(c => c.PostId == claim.PostId && c.Id != claim.Id && c.Status == ClaimStatus.Pending)
            .ToListAsync(ct);

        foreach (var c in othersPending)
            c.Status = ClaimStatus.Standby;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            claimId = claim.Id,
            postId = claim.PostId,
            claimStatus = claim.Status,
            postStatus = claim.Post.Status
        });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var isMod = User.IsInRole("Admin") || User.IsInRole("Moderator");

        var claim = await _db.Claims
            .Include(c => c.Post)
            .Include(c => c.Conversation)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (claim is null)
            return NotFound();

        var isOwner = claim.Post!.CreatedByUserId == userId;

        if (!(isOwner || isMod))
            return Forbid();

        if (claim.Status is not (ClaimStatus.Pending or ClaimStatus.Standby))
            return BadRequest("Solo se puede rechazar una claim en estado Pending o Standby.");

        claim.Status = ClaimStatus.Rejected;
        claim.ResolvedAtUtc = DateTime.UtcNow;

        if (claim.Conversation is not null)
            claim.Conversation.Status = ConversationStatus.ReadOnly;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            claimId = claim.Id,
            postId = claim.PostId,
            claimStatus = claim.Status,
            conversationStatus = claim.Conversation?.Status
        });
    }


}
