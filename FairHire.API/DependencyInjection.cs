using FairHire.Application;
using FairHire.Infrastructure.Postgres;

namespace FairHire.API;


/// This is a placeholder file 
/// for dependency injection setup in the FairHire.API project.
public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Implementation for adding API services
        services.AddPostgresInfrastructure(configuration);
        services.AddApplication(configuration);
    }
}
