using LostUAL.Data.Entities;
using LostUAL.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LostUAL.Api.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(LostUALDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Llaves" },
                new Category { Name = "Carteras" },
                new Category { Name = "Electrónica" },
                new Category { Name = "Mochilas" },
                new Category { Name = "Documentación" }
            );
        }

        if (!await db.Locations.AnyAsync())
        {
            db.Locations.AddRange(
                new CampusLocation { Name = "Biblioteca" },
                new CampusLocation { Name = "Cafetería central" },
                new CampusLocation { Name = "Aulario" },
                new CampusLocation { Name = "Edificio de Ingeniería" },
                new CampusLocation { Name = "Parada de bus" }
            );
        }

        await db.SaveChangesAsync();
    }
}
