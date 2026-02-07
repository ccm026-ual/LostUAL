using LostUAL.Contracts.Claims;
using Microsoft.VisualBasic;

namespace LostUAL.Data.Entities;

public class Claim
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public ItemPost Post { get; set; } = default!;

    public string ClaimantUserId { get; set; } = default!;

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? OwnerConfirmedAtUtc { get; set; }
    public DateTime? ClaimantConfirmedAtUtc { get; set; }
    public DateTime? AutoResolveAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public Conversation Conversation { get; set; } = default!;
}
