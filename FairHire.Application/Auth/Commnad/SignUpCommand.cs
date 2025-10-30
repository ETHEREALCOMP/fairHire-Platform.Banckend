using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad;

public sealed class SignUpCommand(UserManager<User> userManager)
{
    public async Task<Guid> ExecuteAsync(SignUpRequest request, CancellationToken ct)
    {
        // Implementation for sign-up command
        if (request.Password != request.ConfPassword)
            throw new InvalidOperationException("Password and Confirm Password do not match.");

        if (await userManager.FindByEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserName = request.Email, // логін по email
            Email = request.Email,
            Skills = request.Skills,
            TestTasks = request.TestTasks
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));

        var roleToAssign = (request.Role?.Trim().ToLowerInvariant()) switch
        {
            "company" => "company",
            "dev" => "dev",
            _ => throw new NotImplementedException(),
        };

        var addRole = await userManager.AddToRoleAsync(user, roleToAssign);
        if (!addRole.Succeeded)
            throw new InvalidOperationException(string.Join("; ", addRole.Errors.Select(e => e.Description)));

        return user.Id;
    }
}
