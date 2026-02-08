using LostUAL.Contracts.Chat;
using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Reports;
using LostUAL.Data.Entities;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly LostUALDbContext _db;
    public ConversationsController(LostUALDbContext db) => _db = db;

    public sealed class SendMessageRequest
    {
        public string Body { get; set; } = "";
    }

    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var isMod = User.IsInRole("Admin") || User.IsInRole("Moderator");

        var conv = await _db.Conversations
        .Include(c => c.Claim).ThenInclude(cl => cl.Post)
        .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (conv is null)
            return NotFound();

        var info = await _db.Conversations
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Status,
                ClaimId = c.ClaimId,
                ClaimStatus = c.Claim.Status,
                ClaimantId = c.Claim.ClaimantUserId,
                OwnerId = c.Claim.Post.CreatedByUserId,
                c.Claim.OwnerConfirmedAtUtc,
                c.Claim.ClaimantConfirmedAtUtc,
                c.Claim.AutoResolveAtUtc
            })
            .FirstOrDefaultAsync(ct);

        if (info is null)
            return NotFound();

        var isParticipant = isMod || userId == info.ClaimantId || userId == info.OwnerId;
        if (!isParticipant)
            return Forbid();
        var isOwner = userId == info.OwnerId;
        var isClaimant = userId == info.ClaimantId;

        var hasOpenReport = isMod && await _db.ConversationReports
        .AsNoTracking()
        .AnyAsync(r => r.ConversationId == id && r.IsOpen, ct);
        
        if (!(isOwner || isClaimant || hasOpenReport))
            return Forbid();

        var canAccept = isOwner && (info.ClaimStatus == ClaimStatus.Pending || info.ClaimStatus == ClaimStatus.Standby);
        var canReject = (isOwner || isMod) && (info.ClaimStatus == ClaimStatus.Pending || info.ClaimStatus == ClaimStatus.Standby);
        var canSend = (isOwner || isClaimant) && conv.Status == ConversationStatus.Active;
        var canReport = (isOwner || isClaimant);

        var canConfirm =
            info.ClaimStatus == ClaimStatus.Accepted &&
            ((isOwner && info.OwnerConfirmedAtUtc == null) || (isClaimant && info.ClaimantConfirmedAtUtc == null));
        
        var canWithdraw =
            isClaimant
            && (info.ClaimStatus == ClaimStatus.Pending || info.ClaimStatus == ClaimStatus.Standby || info.ClaimStatus == ClaimStatus.Accepted)
            && info.OwnerConfirmedAtUtc == null
            && info.ClaimantConfirmedAtUtc == null;

        var items = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new
            {
                m.Id,
                m.SenderUserId,
                m.Body,
                m.CreatedAtUtc
            })
            .ToListAsync(ct);
        var senderIds = items.Select(x => x.SenderUserId).Distinct().ToList();

        var emails = await _db.Users
            .AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(x => x.Id, x => x.Email, ct);

        var items2 = items.Select(m =>
        {
            emails.TryGetValue(m.SenderUserId, out var email);
            return new MessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderEmail = email,
                Body = m.Body,
                CreatedAtUtc = m.CreatedAtUtc
            };
        }).ToList();

        var now = DateTime.UtcNow;
        if (isOwner) conv.OwnerLastReadAtUtc = now;
        if (isClaimant) conv.ClaimantLastReadAtUtc = now;

        if (isOwner || isClaimant)
            await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            conversationId = id,
            status = info.Status,
            claimId = info.ClaimId,
            claimStatus = info.ClaimStatus,
            canAccept,
            canReject,
            canConfirm,
            canWithdraw,
            canSend,
            canReport,
            ownerConfirmedAtUtc = info.OwnerConfirmedAtUtc,
            claimantConfirmedAtUtc = info.ClaimantConfirmedAtUtc,
            autoResolveAtUtc = info.AutoResolveAtUtc,
            messages = items2
        });

    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var text = (body?.Body ?? "").Trim();
        if (text.Length < 1)
            return BadRequest("Mensaje vacío.");
        if (text.Length > 1000)
            return BadRequest("Mensaje demasiado largo (máx 1000).");

        var isMod = User.IsInRole("Admin") || User.IsInRole("Moderator");

        var conv = await _db.Conversations
            .Include(c => c.Claim)
            .ThenInclude(cl => cl.Post)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (conv is null)
            return NotFound();

        var isOwner = conv.Claim.Post.CreatedByUserId == userId;
        var isClaimant = conv.Claim.ClaimantUserId == userId;

        if (!(isOwner || isClaimant))
            return Forbid();

        if (conv.Status == ConversationStatus.ReadOnly)
            return BadRequest("La conversación está en solo lectura.");

        var msg = new Message
        {
            ConversationId = id,
            SenderUserId = userId,
            Body = text,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Messages.Add(msg);

        var now = DateTime.UtcNow;

        conv.LastMessageAtUtc = now;
        conv.LastMessageByUserId = userId;
        if (isOwner) conv.OwnerLastReadAtUtc = now;
        if (isClaimant) conv.ClaimantLastReadAtUtc = now;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            msg.Id,
            msg.SenderUserId,
            msg.Body,
            msg.CreatedAtUtc
        });
    }

    [HttpPost("{id:int}/report")]
    public async Task<IActionResult> Report(int id, [FromBody] ReportDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var conv = await _db.Conversations
            .Include(c => c.Claim).ThenInclude(cl => cl.Post)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (conv is null) return NotFound();

        var isOwner = conv.Claim.Post.CreatedByUserId == userId;
        var isClaimant = conv.Claim.ClaimantUserId == userId;

        if (!(isOwner || isClaimant))
            return Forbid();

        _db.ConversationReports.Add(new ConversationReport
        {
            ConversationId = id,
            ReporterUserId = userId,
            Reason = (dto?.Reason ?? "").Trim()
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

}
