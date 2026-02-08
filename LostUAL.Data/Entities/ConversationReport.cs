namespace LostUAL.Data.Entities;
using LostUAL.Contracts.Reports;

public sealed class ConversationReport
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public string ReporterUserId { get; set; } = default!;
    public string Reason { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsOpen { get; set; } = true;
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public string? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ModeratorNote { get; set; }
    public string? BlockedUserId { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
}
