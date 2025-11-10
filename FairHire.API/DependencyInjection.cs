using FairHire.API.Options;
using FairHire.API.Problems;
using FairHire.Application;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
        services.AddHttpContextAccessor();
        services.AddCors();
        services.AddSingleton(new RequestLimitsOptions
        {
            ApiMaxBodyBytes = 2L * 1024 * 1024
        });
        services.AddSingleton<Problem>();
    }

    private static void AddAuth(this IServiceCollection services,
        IConfiguration configuration) 
    {
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true; // true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

        services.AddAuthorization(o =>
        {
            o.AddPolicy("developer", p => p.RequireRole("developer"));
            o.AddPolicy("company", p => p.RequireRole("company"));
            o.AddPolicy("devOrCompany", p => p.RequireRole("developer", "company")); // OR
        });
    }
}
