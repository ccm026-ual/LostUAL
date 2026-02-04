using LostUAL.Contracts.Posts;
using System.Security.Claims;

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
    public string? PhotoPath { get; set; }

    public int? WinningClaimId { get; set; } 
    public DateTime? OnClaimAtUtc { get; set; }

    public DateTime? AutoResolveAtUtc { get; set; }
    public bool IsAutoResolvePaused { get; set; } = false; 

    public DateTime? CreatorConfirmedResolvedAtUtc { get; set; }
    public DateTime? ClaimantConfirmedResolvedAtUtc { get; set; }
    public List<Claim> Claims { get; set; } = new();
}

