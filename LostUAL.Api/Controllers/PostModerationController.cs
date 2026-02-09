using LostUAL.Contracts.Chat;
using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Posts;           
using LostUAL.Contracts.Reports;
using LostUAL.Data.Entities;
using LostUAL.Data.Identity;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/moderation/posts")]
[Authorize(Roles = "Moderator,Admin")]
public sealed class PostModerationController : ControllerBase
{
    private readonly LostUALDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PostModerationController(LostUALDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("reports")]
    public async Task<ActionResult<List<PostReportListItemDto>>> List([FromQuery] ReportStatus? status = null, CancellationToken ct = default)
    {
        var q = _db.PostReports
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .AsQueryable();

        if (status is not null)
            q = q.Where(r => r.Status == status);

        var reports = await q.ToListAsync(ct);

        var result = new List<PostReportListItemDto>(reports.Count);

        foreach (var r in reports)
        {
            var reporter = await _userManager.FindByIdAsync(r.ReporterUserId);

            ApplicationUser? blocked = null;
            if (!string.IsNullOrWhiteSpace(r.BlockedUserId))
                blocked = await _userManager.FindByIdAsync(r.BlockedUserId);

            var post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == r.PostId, ct);
            var postStatus = post?.Status ?? PostStatus.Open;

            result.Add(new PostReportListItemDto
            {
                Id = r.Id,
                PostId = r.PostId,
                CreatedAtUtc = r.CreatedAtUtc,
                Status = r.Status,
                Reason = r.Reason,

                ReporterEmail = reporter?.Email ?? r.ReporterUserId,

                Action = r.Action,
                PostStatus = postStatus,

                BlockedEmail = blocked?.Email,
                LockoutEndUtc = r.LockoutEndUtc
            });
        }

        return Ok(result);
    }

    [HttpGet("reports/{reportId:int}/summary")]
    public async Task<ActionResult<PostReportSummaryDto>> Summary(int reportId, CancellationToken ct = default)
    {
        var r = await _db.PostReports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reportId, ct);

        if (r is null) return NotFound();

        var reporter = await _userManager.FindByIdAsync(r.ReporterUserId);

        ApplicationUser? blocked = null;
        if (!string.IsNullOrWhiteSpace(r.BlockedUserId))
            blocked = await _userManager.FindByIdAsync(r.BlockedUserId);

        var post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == r.PostId, ct);

        return Ok(new PostReportSummaryDto
        {
            Id = r.Id,
            PostId = r.PostId,
            CreatedAtUtc = r.CreatedAtUtc,
            Status = r.Status,
            Reason = r.Reason,

            ReporterEmail = reporter?.Email ?? r.ReporterUserId,

            Action = r.Action,
            PostStatus = post?.Status ?? PostStatus.Open,
            ClosedAtUtc = post?.ClosedAtUtc,
            ClosedReason = post?.ClosedReason,

            BlockedEmail = blocked?.Email,
            LockoutEndUtc = r.LockoutEndUtc,

            ModeratorNote = r.ModeratorNote,
            ResolvedAtUtc = r.ResolvedAtUtc
        });
    }

    [HttpPost("reports/{reportId:int}/dismiss")]
    public async Task<IActionResult> Dismiss(int reportId, [FromBody] DismissReportRequest req, CancellationToken ct = default)
    {
        var modUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(modUserId)) return Unauthorized();

        var r = await _db.PostReports.FirstOrDefaultAsync(x => x.Id == reportId, ct);
        if (r is null) return NotFound();
        if (r.Status != ReportStatus.Open) return BadRequest("El reporte ya no está abierto.");

        r.Status = ReportStatus.Dismissed;
        r.Action = PostReportAction.Dismissed;

        r.ResolvedAtUtc = DateTime.UtcNow;
        r.ResolvedByUserId = modUserId;
        r.ModeratorNote = req?.Note;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("reports/{reportId:int}/close")]
    public async Task<IActionResult> ClosePost(int reportId, [FromBody] DismissReportRequest req, CancellationToken ct = default)
    {
        var modUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(modUserId)) return Unauthorized();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var report = await _db.PostReports.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report is null) return NotFound();
        if (report.Status != ReportStatus.Open) return BadRequest("El reporte ya no está abierto.");

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == report.PostId, ct);
        if (post is null) return BadRequest("Post no encontrado.");

        await ClosePostAndRejectClaimsAsync(post, modUserId, req?.Note, ct);

        report.Status = ReportStatus.ActionTaken;
        report.Action = PostReportAction.ClosePost;
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = modUserId;
        report.ModeratorNote = req?.Note;

        await ResolveOtherOpenReportsAsync(post.Id, report.Id, modUserId, req?.Note, ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }



    [HttpPost("reports/{reportId:int}/close-block")]
    public async Task<IActionResult> ClosePostAndBlock(int reportId, [FromBody] BlockUserRequest req, CancellationToken ct = default)
    {
        var modUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(modUserId)) return Unauthorized();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var report = await _db.PostReports.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report is null) return NotFound();
        if (report.Status != ReportStatus.Open) return BadRequest("El reporte ya no está abierto.");

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == report.PostId, ct);
        if (post is null) return BadRequest("Post no encontrado.");

        var targetUserId = post.CreatedByUserId;
        if (string.IsNullOrWhiteSpace(targetUserId)) return BadRequest("Post sin CreatedByUserId.");

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user is null) return BadRequest("Usuario objetivo no encontrado.");

        await ClosePostAndRejectClaimsAsync(post, modUserId, req?.Note, ct);

        var days = req?.Days ?? 7;
        if (days < 1) days = 1;

        await _userManager.SetLockoutEnabledAsync(user, true);

        var desiredEnd = DateTimeOffset.UtcNow.AddDays(days);
        var currentEnd = await _userManager.GetLockoutEndDateAsync(user);

        var finalEnd = (currentEnd is null || currentEnd < desiredEnd) ? desiredEnd : currentEnd.Value;

        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, finalEnd);
        if (!lockoutResult.Succeeded) return BadRequest("No se pudo aplicar el lockout.");

        report.Status = ReportStatus.ActionTaken;
        report.Action = PostReportAction.ClosePostAndBlockUser; 
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = modUserId;
        report.ModeratorNote = req?.Note;

        report.BlockedUserId = targetUserId;
        report.LockoutEndUtc = finalEnd.UtcDateTime;

        await ResolveOtherOpenReportsAsync(post.Id, report.Id, modUserId, req?.Note, ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }



    private async Task ClosePostAndRejectClaimsAsync(ItemPost post, string modUserId, string? closedReason, CancellationToken ct)
    {
        if (post.Status != PostStatus.Closed)
        {
            post.Status = PostStatus.Closed;
            post.ClosedAtUtc = DateTime.UtcNow;
            post.ClosedByUserId = modUserId;
            post.ClosedReason = string.IsNullOrWhiteSpace(closedReason)
                ? "Cerrado por moderación"
                : closedReason!.Trim();
        }

        var claims = await _db.Claims
            .Include(c => c.Conversation)
            .Where(c => c.PostId == post.Id && c.IsActive)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var c in claims)
        {

            c.Status = ClaimStatus.Rejected;
            c.IsActive = false;
            c.ResolvedAtUtc = now;
            c.AutoResolveAtUtc = null;
            c.ExpiresAtUtc = now;

            if (c.Conversation is not null)
            {
                c.Conversation.Status = ConversationStatus.ReadOnly;
            }
        }
    }

    private async Task ResolveOtherOpenReportsAsync(int postId, int actedReportId, string modUserId, string? note, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var others = await _db.PostReports
            .Where(r => r.PostId == postId && r.Id != actedReportId && r.Status == ReportStatus.Open)
            .ToListAsync(ct);

        foreach (var r in others)
        {
            r.Status = ReportStatus.Dismissed;
            r.ResolvedAtUtc = now;
            r.ResolvedByUserId = modUserId;
            r.ModeratorNote = $"Resuelto automáticamente: ya se tomó acción en el reporte #{actedReportId}." +
                              (string.IsNullOrWhiteSpace(note) ? "" : $" Nota: {note!.Trim()}");
        }
    }

}

