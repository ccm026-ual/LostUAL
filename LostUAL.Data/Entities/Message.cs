namespace LostUAL.Data.Entities;

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    public string SenderUserId { get; set; } = default!;

    public string Body { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
