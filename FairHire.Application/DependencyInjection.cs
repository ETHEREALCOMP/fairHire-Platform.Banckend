using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Query;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.AssessmentFeature.Command;
using FairHire.Application.Feature.AssessmentFeature.Query;
using FairHire.Application.Feature.SimulationFeature.Command;
using FairHire.Application.Feature.SimulationFeature.Query;
using FairHire.Application.Feature.SubmissionFeature.Command;
using FairHire.Application.Feature.SubmissionFeature.Query;
using FairHire.Application.Feature.TaskFeature.Command;
using FairHire.Application.Feature.TaskFeature.Query;
using FairHire.Application.Feature.WorkItemFeature.Command;
using FairHire.Application.Jwt;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;
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
        services.AddScoped<SignInManager<User>>();
        services.AddScoped<ICurrentUser, CurrentUserFromHttp>();

    }

    private static IServiceCollection AddCommand(this IServiceCollection services)
    {
        services.AddScoped<SignOutCommand>();

        services.AddScoped<UserSignInCommand>();
        services.AddScoped<UserSignUpCommand>();
        services.AddScoped<UpdateUserCommand>();

        services.AddScoped<CreateAssessmentCommand>();
        services.AddScoped<ActivateSimulationCommand>();
        services.AddScoped<CreateSimulationCommand>();
        services.AddScoped<FinishSimulationCommand>();

        services.AddScoped<CreateSubmissionCommand>();

        services.AddScoped<ArchiveTaskTemplateCommand>();
        services.AddScoped<CreateTaskTemplateCommand>();
        services.AddScoped<DeleteTaskTemplateCommand>();
        services.AddScoped<UpdateTaskTemplateCommand>();

        services.AddScoped<CreateWorkItemCommand>();
        services.AddScoped<UpdateWorkItemStatusCommand>();

        return services;
    }

    private static IServiceCollection AddQuery(this IServiceCollection services)
    {
        services.AddScoped<GetUserDataQuery>();
        services.AddScoped<GetAssessmentByIdQuery>();
        services.AddScoped<GetSimulationByIdQuery>();
        services.AddScoped<GetSubmissionByIdQuery>();
        services.AddScoped<GetSubmissionsBySimulationQuery>();
        services.AddScoped<GetTaskTemplateByIdQuery>();
        services.AddScoped<GetAllTaskTemplatesQuery>();

        return services;
    }
}
