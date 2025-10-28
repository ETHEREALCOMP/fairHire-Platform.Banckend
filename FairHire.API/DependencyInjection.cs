using FairHire.Application;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FairHire.API;

/// This is a placeholder file 
/// for dependency injection setup in the FairHire.API project.
public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Implementation for adding API services
        services.AddPostgresInfrastructure(configuration);
        services.AddApplication(configuration);
        services.AddAuth(configuration);
    }

    private static void AddAuth(this IServiceCollection services,
        IConfiguration configuration) 
    {
        services.AddAuthentication().AddJwtBearer();
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Issuer"],
                ValidAudience = configuration["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SigningKey"])),
                ClockSkew = TimeSpan.Zero
            };
        });
    }
}
