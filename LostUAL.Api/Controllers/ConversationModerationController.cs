using LostUAL.Contracts.Chat;
using LostUAL.Contracts.Reports;
using LostUAL.Data.Entities;
using LostUAL.Data.Persistence;
using LostUAL.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

public sealed class BlockUserRequest
{
    public int Days { get; set; } = 7;
    public string? Note { get; set; }
}

public sealed class DismissReportRequest
{
    public string? Note { get; set; }
}

[ApiController]
[Route("api/moderation/conversations")]
[Authorize(Roles = "Moderator,Admin")]
public class ConversationModerationController : ControllerBase
{
    private readonly LostUALDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConversationModerationController(LostUALDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }
    /*
    [HttpGet("reports")]
    public async Task<ActionResult<List<object>>> List([FromQuery] ReportStatus? status = null, CancellationToken ct = default)
    {
        var q = _db.ConversationReports
            .AsNoTracking()
            .Include(r => r.Conversation)
                .ThenInclude(c => c.Claim)
                    .ThenInclude(cl => cl.Post)
            .OrderByDescending(r => r.CreatedAtUtc)
            .AsQueryable();

        if (status is not null)
            q = q.Where(r => r.Status == status);

        var items = await q.Select(r => new
        {
            r.Id,
            r.ConversationId,
            PostId = r.Conversation.Claim.PostId,
            PostTitle = r.Conversation.Claim.Post.Title,
            r.ReporterUserId,
            r.Reason,
            r.CreatedAtUtc,
            r.Status,
            r.ResolvedAtUtc,
            r.ResolvedByUserId,
            r.ModeratorNote,
            r.BlockedUserId,
            r.LockoutEndUtc
        }).ToListAsync(ct);

        return Ok(items);
    }
    */
    [HttpPost("reports/{reportId:int}/dismiss")]
    public async Task<IActionResult> Dismiss(int reportId, [FromBody] DismissReportRequest req, CancellationToken ct = default)
    {
        var modUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(modUserId))
            return Unauthorized();

        var report = await _db.ConversationReports
            .Include(r => r.Conversation)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);

        if (report is null) return NotFound();
        if (report.Status != ReportStatus.Open) return BadRequest("El reporte ya no está abierto.");

        report.Status = ReportStatus.Dismissed;
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = modUserId;
        report.ModeratorNote = req?.Note;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("reports/{reportId:int}/block")]
    public async Task<IActionResult> BlockUser(int reportId, [FromBody] BlockUserRequest req, CancellationToken ct = default)
    {
        var modUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(modUserId))
            return Unauthorized();

        var report = await _db.ConversationReports
            .Include(r => r.Conversation)
                .ThenInclude(c => c.Claim)
                    .ThenInclude(cl => cl.Post)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);

        if (report is null) return NotFound();
        if (report.Status != ReportStatus.Open) return BadRequest("El reporte ya no está abierto.");

        var claim = report.Conversation.Claim;
        var ownerId = claim.Post.CreatedByUserId;
        var claimantId = claim.ClaimantUserId;

        string? targetUserId = null;

        if (report.ReporterUserId == ownerId)
            targetUserId = claimantId;
        else if (report.ReporterUserId == claimantId)
            targetUserId = ownerId;

        if (string.IsNullOrWhiteSpace(targetUserId))
            return BadRequest("No se pudo determinar el usuario objetivo a bloquear.");

        var days = req?.Days ?? 7;
        if (days < 1) days = 1;

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user is null)
            return BadRequest("Usuario objetivo no encontrado.");

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;
            await _userManager.UpdateAsync(user);
        }

        var lockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!result.Succeeded)
            return BadRequest("No se pudo aplicar el lockout.");

        report.Status = ReportStatus.ActionTaken;
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = modUserId;
        report.ModeratorNote = req?.Note;

        report.BlockedUserId = targetUserId;
        report.LockoutEndUtc = lockoutEnd.UtcDateTime;

        report.Conversation.Status = ConversationStatus.ReadOnly; 

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
    [HttpGet("reports")]
    public async Task<ActionResult<List<ConversationReportListItemDto>>> List([FromQuery] ReportStatus? status = null, CancellationToken ct = default)
    {
        var q = _db.ConversationReports
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .AsQueryable();

        if (status is not null)
            q = q.Where(r => r.Status == status);

        var items = await q.Select(r => new ConversationReportListItemDto
        {
            Id = r.Id,
            ConversationId = r.ConversationId,
            CreatedAtUtc = r.CreatedAtUtc,
            Status = r.Status,
            Reason = r.Reason,

            ReporterEmail = _db.Users
                .Where(u => u.Id == r.ReporterUserId)
                .Select(u => u.Email!)
                .FirstOrDefault() ?? r.ReporterUserId,

            BlockedEmail = r.BlockedUserId == null ? null :
                _db.Users.Where(u => u.Id == r.BlockedUserId)
                    .Select(u => u.Email!)
                    .FirstOrDefault(),

            LockoutEndUtc = r.LockoutEndUtc
        }).ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("reports/{reportId:int}")]
    public async Task<ActionResult<ConversationReportDetailDto>> GetReport(int reportId, CancellationToken ct = default)
    {
        var report = await _db.ConversationReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);

        if (report is null) return NotFound();

        var messages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == report.ConversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new ModerationConversationMessageDto
            {
                Id = m.Id,
                CreatedAtUtc = m.CreatedAtUtc,
                SenderEmail = _db.Users
                    .Where(u => u.Id == m.SenderUserId)
                    .Select(u => u.Email!)
                    .FirstOrDefault() ?? m.SenderUserId,
                Content = m.Body
            })
            .ToListAsync(ct);

        var dto = new ConversationReportDetailDto
        {
            Id = report.Id,
            ConversationId = report.ConversationId,
            CreatedAtUtc = report.CreatedAtUtc,
            Status = report.Status,
            Reason = report.Reason,

            ReporterEmail = _db.Users
                .Where(u => u.Id == report.ReporterUserId)
                .Select(u => u.Email!)
                .FirstOrDefault() ?? report.ReporterUserId,

            BlockedEmail = report.BlockedUserId == null ? null :
                _db.Users.Where(u => u.Id == report.BlockedUserId)
                    .Select(u => u.Email!)
                    .FirstOrDefault(),

            LockoutEndUtc = report.LockoutEndUtc,
            ModeratorNote = report.ModeratorNote,
            ResolvedAtUtc = report.ResolvedAtUtc,
            Messages = messages
        };

        return Ok(dto);
    }

    [HttpGet("reports/{reportId:int}/summary")]
    public async Task<ActionResult<ConversationReportSummaryDto>> GetSummary(int reportId, CancellationToken ct = default)
    {
        var report = await _db.ConversationReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);

        if (report is null)
            return NotFound();

        var reporter = await _userManager.FindByIdAsync(report.ReporterUserId);
        ApplicationUser? blocked = null;

        if (!string.IsNullOrWhiteSpace(report.BlockedUserId))
            blocked = await _userManager.FindByIdAsync(report.BlockedUserId);

        var dto = new ConversationReportSummaryDto
        {
            Id = report.Id,
            ConversationId = report.ConversationId,
            CreatedAtUtc = report.CreatedAtUtc,
            Status = report.Status,
            Reason = report.Reason,

            ReporterEmail = reporter?.Email ?? report.ReporterUserId,

            BlockedEmail = blocked?.Email,
            LockoutEndUtc = report.LockoutEndUtc,

            ModeratorNote = report.ModeratorNote,
            ResolvedAtUtc = report.ResolvedAtUtc
        };

        return Ok(dto);
    }


}
