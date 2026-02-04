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
    DateTime CreatedAtUtc
);
