using LostUAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LostUAL.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LostUAL.Data.Persistence;

public class LostUALDbContext : IdentityDbContext<ApplicationUser>
{
    public LostUALDbContext(DbContextOptions<LostUALDbContext> options) : base(options) { }

    public DbSet<ItemPost> Posts => Set<ItemPost>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CampusLocation> Locations => Set<CampusLocation>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ConversationReport> ConversationReports => Set<ConversationReport>();
    public DbSet<PostReport> PostReports => Set<PostReport>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            d => d.ToString("yyyy-MM-dd"),
            s => DateOnly.Parse(s)
        );

        var dateOnlyComparer = new ValueComparer<DateOnly>(
            (l, r) => l.DayNumber == r.DayNumber,
            d => d.GetHashCode(),
            d => DateOnly.FromDayNumber(d.DayNumber)
        );

    
        modelBuilder.Entity<ItemPost>()
            .Property(p => p.DateApprox)
            .HasConversion(dateOnlyConverter)
            .Metadata.SetValueComparer(dateOnlyComparer);

        modelBuilder.Entity<ItemPost>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<CampusLocation>()
            .HasIndex(l => l.Name)
            .IsUnique();

        // Post -> Claims (1:N)
        modelBuilder.Entity<ItemPost>()
            .HasMany(p => p.Claims)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        /*modelBuilder.Entity<Claim>()
            .HasIndex(c => new { c.PostId, c.ClaimantUserId })
            .IsUnique();*/
        modelBuilder.Entity<Claim>()
            .HasIndex(c => new { c.PostId, c.ClaimantUserId })
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        // Claim -> Conversation (1:1)
        modelBuilder.Entity<Claim>()
            .HasOne(c => c.Conversation)
            .WithOne(conv => conv.Claim)
            .HasForeignKey<Conversation>(conv => conv.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Conversation -> Messages (1:N)
        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => c.ClaimId)
            .IsUnique();

        modelBuilder.Entity<ItemPost>()
            .HasOne<Claim>()
            .WithMany()
            .HasForeignKey(p => p.WinningClaimId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ConversationReport>()
            .HasOne(r => r.Conversation)
            .WithMany()
            .HasForeignKey(r => r.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostReport>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason)
                .HasMaxLength(1000);
            e.HasOne(x => x.Post)
                .WithMany() 
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Action).HasConversion<int>();
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => new { x.PostId, x.Status });
            e.HasIndex(x => new { x.PostId, x.ReporterUserId, x.Status });
        });


        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ConversationId, m.CreatedAtUtc });

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.ReadAtUtc });

        modelBuilder.Entity<Report>()
            .HasIndex(r => new { r.PostId, r.CreatedAtUtc });

        modelBuilder.Entity<ItemPost>()
            .HasIndex(p => new { p.Status, p.CreatedAtUtc });
    }
}
