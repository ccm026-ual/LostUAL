using LostUAL.Contracts.Reports;

namespace LostUAL.Data.Entities;

public sealed class PostReport
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public ItemPost Post { get; set; } = default!;

    public string ReporterUserId { get; set; } = default!;
    public string Reason { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public string? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ModeratorNote { get; set; }

    public string? BlockedUserId { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public PostReportAction Action { get; set; } = PostReportAction.None;

}
