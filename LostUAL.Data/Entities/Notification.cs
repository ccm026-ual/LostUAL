using LostUAL.Contracts.Notifications;

namespace LostUAL.Data.Entities;

public class Notification
{
    public int Id { get; set; }
    public string? UserId { get; set; }

    public NotificationType Type { get; set; }

    public int? PostId { get; set; }
    public int? ClaimId { get; set; }
    public int? ConversationId { get; set; }

    public string? Text { get; set; } 

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}
