using LostUAL.Contracts.Posts;

namespace LostUAL.Data.Entities;

public class ItemPost
{
    public int Id { get; set; }

    public PostType Type { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int LocationId { get; set; }
    public CampusLocation? Location { get; set; }

    public DateOnly DateApprox { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Open;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

}
