using FairHire.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FairHire.Infrastructure.Postgres;

/// This is a placeholder file 
/// for dependency injection setup in the FairHire.API project.
public static class DependencyInjection
{
    public static void AddPostgresInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Implementation for adding Postgres infrastructure services
        services.AddDbContextPool<FairHireDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        }).AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<FairHireDbContext>();

    }
}
