using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad
{
    public sealed class EditUserCommand(UserManager<User> userManager)
    {
        public async Task ExecuteAsync(Guid userId, EditUserDataRequest request, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new Exception("User not found");
            }
            user.Email = request.Email;
            user.Name = request.Name;
            user.Skills = request.Skills;
            await userManager.UpdateAsync(user);
        }
    }
}
