using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad;

public sealed class SignUpCommand(UserManager<User> userManager)
{
    public async Task<Guid> ExecuteAsync(SignUpRequest request, CancellationToken ct)
    {
        // Implementation for sign-up command
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var newUser = new User
        {
            Name = request.Name,
            UserName = request.Name,
            Email = request.Email,
            Password = request.Password,
        };


        if (request.Password != request.ConfPassword)
        {
            throw new InvalidOperationException("Password and Confirm Password do not match.");
        } 

        var res = await userManager.CreateAsync(newUser, request.Password);

        if (res.Succeeded == false)
        {
            res.Errors.ToList().ForEach(e => 
            {
                Console.WriteLine($"Error Code: {e.Code}, Description: {e.Description}");
            });

            throw new InvalidOperationException("Failed to create user.");
        }

        return newUser.Id;
    }
}
