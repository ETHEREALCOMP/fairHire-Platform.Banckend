using FairHire.Application.Auth.Models.Request.Companies;
using FairHire.Application.Base.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad.Companies;

public sealed class CompanySignUpCommand(AppDbContext context)
{
    public async Task<BaseResponse> ExecuteAsync(CompanySignUpRequest request, CancellationToken ct)
    {
        // Implementation for company sign-up command
        if (request.Password != request.ConfPassword)
            throw new InvalidOperationException("Password and Confirm Password do not match.");

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserEmail = request.Email,
            Address = request.Address,
            Website = request.Website,
            UserRole = request.Role,
            Password = request.Password,
        };

        context.Add(company);
        await context.SaveChangesAsync();

        var roleToAssign = (request.Role?.Trim().ToLowerInvariant()) switch
        {
            "company" => "company",
            _ => throw new NotImplementedException(),
        };

        return new() { Id = company.Id };
    }
}
