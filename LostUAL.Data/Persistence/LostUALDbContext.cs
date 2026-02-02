using LostUAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LostUAL.Data.Persistence;

public class LostUALDbContext : DbContext
{
    public LostUALDbContext(DbContextOptions<LostUALDbContext> options) : base(options) { }

    public DbSet<ItemPost> Posts => Set<ItemPost>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CampusLocation> Locations => Set<CampusLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DateOnly en SQLite: lo guardamos como string "yyyy-MM-dd"
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

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<CampusLocation>()
            .HasIndex(l => l.Name)
            .IsUnique();
    }
}
