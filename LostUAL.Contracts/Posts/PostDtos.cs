namespace LostUAL.Contracts.Posts;

public enum PostType
{
    Lost = 0,
    Found = 1
}

public enum PostStatus
{
    Open = 0,
    InClaim = 1,
    Resolved = 2,
    Closed = 3
}

public sealed record PostListItemDto(
    int Id,
    PostType Type,
    string Title,
    string Category,
    string Location,
    DateOnly DateApprox,
    PostStatus Status,
    DateTime CreatedAtUtc
);
public sealed record MyPostListItemDto(
    int PostId,
    string Title,
    PostType Type,
    PostStatus Status,
    DateTime CreatedAtUtc,
    int PendingClaimsCount,
    int? AcceptedConversationId
);

public sealed record CreatePostRequest(
    PostType Type,
    string Title,
    string Description,
    int CategoryId,
    int LocationId,
    DateOnly DateApprox
);

public sealed record PostDetailDto(
    int Id,
    PostType Type,
    string Title,
    string Description,
    int CategoryId,
    string Category,
    int LocationId,
    string Location,
    DateOnly DateApprox,
    PostStatus Status,
    DateTime CreatedAtUtc,
    string CreatedByUserId
);

public sealed record PostPreviewDto(
    int Id,
    string Title,
    string Description,
    DateTime CreatedAtUtc
    );

public sealed record PostsQuery(
    int Page = 1,
    int PageSize = 10,
    PostType? Type = null,
    PostStatus? Status = null,
    int? CategoryId = null,
    int? LocationId = null,
    DateTime? FromCreatedAtUtc = null,
    DateTime? ToCreatedAtUtc = null
);

public sealed class ClosePostRequest
{
    public string? Reason { get; set; }
}

