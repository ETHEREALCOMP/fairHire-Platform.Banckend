using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Query;
using FairHire.Application.Feature.TestTaskFeature.Commands;
using FairHire.Application.Feature.TestTaskFeature.Queries;
using FairHire.Application.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FairHire.Application;

/// This is a placeholder file 
/// for dependency injection setup in the FairHire.API project.
public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Implementation for adding application services
        services.AddCommand();
        services.AddQuery();
        services.AddSingleton<JwtService>();
    }

    private static IServiceCollection AddCommand(this IServiceCollection services)
    {
        services.AddScoped<SignInCommand>();
        services.AddScoped<SignUpCommand>();
        services.AddScoped<SignOutCommand>();
        services.AddScoped<EditUserCommand>();
        services.AddScoped<CreateTestTaskCommand>();
        services.AddScoped<UpdateTestTaskCommand>();
        services.AddScoped<DeleteTestTaskCommand>();
        
        return services;
    }

    private static IServiceCollection AddQuery(this IServiceCollection services)
    {
        services.AddScoped<GetAllTestTaskQuery>();
        services.AddScoped<GetByIdTestTaskQuery>();
        services.AddScoped<GetUserDataQuery>();

        return services;
    }
}
