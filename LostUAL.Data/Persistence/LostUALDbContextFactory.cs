using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LostUAL.Data.Persistence;

public class LostUALDbContextFactory : IDesignTimeDbContextFactory<LostUALDbContext>
{
    public LostUALDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LostUALDbContext>();

        // Para desarrollo: SQLite local
        optionsBuilder.UseSqlite("Data Source=lostual.db");

        return new LostUALDbContext(optionsBuilder.Options);
    }
}
