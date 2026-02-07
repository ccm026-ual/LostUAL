using LostUAL.Contracts.Chat;
using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Posts;
using LostUAL.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LostUAL.Api.Services;

public sealed class ClaimAutoResolveService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ClaimAutoResolveService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnce(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LostUALDbContext>();

        var now = DateTime.UtcNow;

        var due = await db.Claims
            .Include(c => c.Post)
            .Include(c => c.Conversation)
            .Where(c => c.Status == ClaimStatus.Accepted &&
                        c.AutoResolveAtUtc != null &&
                        c.AutoResolveAtUtc <= now)
            .ToListAsync(ct);

        if (due.Count == 0) return;

        foreach (var claim in due)
        {
            claim.Status = ClaimStatus.Resolved;
            claim.IsActive = false;
            claim.ResolvedAtUtc = now;

            claim.Post!.Status = PostStatus.Resolved;

            if (claim.Conversation is not null)
                claim.Conversation.Status = ConversationStatus.ReadOnly;

            var others = await db.Claims
                .Include(c => c.Conversation)
                .Where(c => c.PostId == claim.PostId && c.Id != claim.Id &&
                            (c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Standby))
                .ToListAsync(ct);

            foreach (var c in others)
            {
                c.Status = ClaimStatus.Rejected;
                c.IsActive = false;
                c.ResolvedAtUtc = now;
                if (c.Conversation is not null)
                    c.Conversation.Status = ConversationStatus.ReadOnly;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
