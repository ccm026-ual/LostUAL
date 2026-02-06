using LostUAL.Contracts.Claims;
using Microsoft.VisualBasic;

namespace LostUAL.Data.Entities;

public class Claim
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public ItemPost? Post { get; set; }

    public string ClaimantUserId { get; set; } = default!;

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public Conversation? Conversation { get; set; }
}
