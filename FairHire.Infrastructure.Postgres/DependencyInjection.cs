using FairHire.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FairHire.Infrastructure.Postgres;

/// This is a placeholder file 
/// for dependency injection setup in the FairHire.API project.
public static class DependencyInjection
{
    public static void AddPostgresInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FairHireDbContext>(options =>
        {
            // 1. Перший пріоритет – стандартний connection string
            var raw = configuration.GetConnectionString("DefaultConnection")
                      ?? configuration["ConnectionStrings:DefaultConnection"]
                      // 2. Фолбек – Railway-style DATABASE_URL
                      ?? configuration["DATABASE_URL"];

            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Connection string 'DefaultConnection' or 'DATABASE_URL' is not configured.");

            // 3. Якщо рядок у форматі postgres://... – конвертуємо у формат Npgsql
            var connectionString = raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                ? ConvertPostgresUrlToNpgsql(raw)
                : raw;

            options.UseNpgsql(connectionString);
        });

        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<FairHireDbContext>();
    }

    private static string ConvertPostgresUrlToNpgsql(string url)
    {
        // url типу: postgres://user:password@host:5432/dbname
        var uri = new Uri(url);

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}
