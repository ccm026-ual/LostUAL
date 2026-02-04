namespace LostUAL.Data.Entities;

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    public string? SenderUserId { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
