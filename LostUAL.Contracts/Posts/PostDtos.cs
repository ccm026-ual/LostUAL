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
    Closed = 2
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
