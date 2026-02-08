namespace LostUAL.Data.Entities;

public sealed class ConversationReport
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public string ReporterUserId { get; set; } = default!;
    public string Reason { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsOpen { get; set; } = true;
}
