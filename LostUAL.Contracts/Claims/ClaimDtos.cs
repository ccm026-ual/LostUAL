using LostUAL.Contracts.Posts;

namespace LostUAL.Contracts.Claims;

public enum ClaimStatus
{
    Pending = 0,
    Accepted = 1,
    Standby = 2,
    Withdrawn = 3,
    Rejected = 4,
    Expired = 5
}

public sealed class CreateClaimRequest
{
    public string Message { get; set; } = "";
}

public sealed record MyClaimListItemDto(
    int ClaimId,
    int PostId,
    string PostTitle,
    PostType PostType,
    ClaimStatus Status,
    DateTime CreatedAtUtc,
    int? ConversationId
);

public sealed record InboxClaimListItemDto(
    int ClaimId,
    int PostId,
    string PostTitle,
    PostType PostType,
    ClaimStatus Status,
    DateTime CreatedAtUtc,
    string ClaimantUserId,
    string? ClaimantEmail,
    int? ConversationId
);
