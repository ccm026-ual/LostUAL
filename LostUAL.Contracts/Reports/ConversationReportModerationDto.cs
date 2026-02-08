namespace LostUAL.Contracts.Reports;

public enum ReportStatus
{
    Open = 0,
    Dismissed = 1,
    ActionTaken = 2
}
public sealed class ConversationReportModerationDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }

    public int PostId { get; set; }
    public string PostTitle { get; set; } = "";

    public string ReporterUserId { get; set; } = "";
    public string Reason { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus? Status { get; set; } = ReportStatus.Open; 

    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolvedByUserId { get; set; }
    public string? ModeratorNote { get; set; }

    public string? BlockedUserId { get; set; }
    public DateTime? LockoutEndUtc { get; set; }

}

public sealed class ConversationReportListItemDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus Status { get; set; }
    public string Reason { get; set; } = "";
    public string ReporterEmail { get; set; } = "";
    public string? BlockedEmail { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
}

public sealed class ConversationReportDetailDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus Status { get; set; }

    public string Reason { get; set; } = "";

    public string ReporterEmail { get; set; } = "";

    public string? BlockedEmail { get; set; }
    public DateTime? LockoutEndUtc { get; set; }

    public string? ModeratorNote { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public List<ModerationConversationMessageDto> Messages { get; set; } = [];
}

public sealed class ModerationConversationMessageDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string SenderEmail { get; set; } = "";
    public string Content { get; set; } = "";
}

public sealed class ConversationReportSummaryDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public ReportStatus Status { get; set; }

    public string Reason { get; set; } = "";

    public string ReporterEmail { get; set; } = "";

    public string? BlockedEmail { get; set; }
    public DateTime? LockoutEndUtc { get; set; }

    public string? ModeratorNote { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
