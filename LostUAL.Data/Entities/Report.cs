namespace LostUAL.Data.Entities;

public class Report
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public ItemPost? Post { get; set; }

    public string? ReporterUserId { get; set; }
    public string? ReportedUserId { get; set; }

    public required string Reason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
