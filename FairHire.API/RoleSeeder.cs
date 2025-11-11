using Microsoft.AspNetCore.Identity;

namespace FairHire.API;

public static class RoleSeeder
{
    private static readonly string[] Roles = { "Developer", "Company", "Admin", "Candidate" };

    public static async Task SeedAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}