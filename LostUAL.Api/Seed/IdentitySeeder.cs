using LostUAL.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace LostUAL.Api.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["User", "Moderator", "Admin"];

        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var createRes = await userManager.CreateAsync(admin, password);
            if (!createRes.Succeeded)
            {
                var msg = string.Join(" | ", createRes.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear el admin inicial: {msg}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");

        if (!await userManager.IsInRoleAsync(admin, "User"))
            await userManager.AddToRoleAsync(admin, "User");
    }
}
