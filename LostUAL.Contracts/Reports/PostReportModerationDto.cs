using LostUAL.Contracts.Posts;

namespace LostUAL.Contracts.Reports;

public enum PostReportAction
{
    None = 0,
    Dismissed = 1,
    ClosePost = 2,
    ClosePostAndBlockUser = 3
}
public sealed class PostReportListItemDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus Status { get; set; }
    public string Reason { get; set; } = "";
    public string ReporterEmail { get; set; } = "";
    public string? BlockedEmail { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public PostReportAction Action { get; set; } = PostReportAction.None;
    public PostStatus PostStatus { get; set; }
}

public sealed class PostReportSummaryDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus Status { get; set; }
    public string Reason { get; set; } = "";
    public string ReporterEmail { get; set; } = "";
    public string? BlockedEmail { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public string? ModeratorNote { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public PostReportAction Action { get; set; } = PostReportAction.None;
    public PostStatus PostStatus { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedReason { get; set; }
}
