using LostUAL.Contracts.Chat;

namespace LostUAL.Data.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public Claim Claim { get; set; } = default!;
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<Message> Messages { get; set; } = new();
    public DateTime? LastMessageAtUtc { get; set; }
    public string? LastMessageByUserId { get; set; }
    public DateTime? OwnerLastReadAtUtc { get; set; }
    public DateTime? ClaimantLastReadAtUtc { get; set; }

}
