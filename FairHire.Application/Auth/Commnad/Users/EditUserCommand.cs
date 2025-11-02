using FairHire.Application.Auth.Models.Request.Users;
using FairHire.Application.Base.Response;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad.Users
{
    public sealed class EditUserCommand(UserManager<User> userManager)
    {
        public async Task<BaseResponse> ExecuteAsync(Guid userId, EditUserDataRequest request, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new Exception("User not found");
            }
            user.Email = string.IsNullOrWhiteSpace(request.Email) 
                ? user.Email : request.Email;

            user.Name = string.IsNullOrWhiteSpace(request.Name)
                ? user.Name : request.Name;

            user.Skills = (request.Skills != null && request.Skills.Count > 0) 
                ? request.Skills : user.Skills;

            user.PasswordHash = string.IsNullOrWhiteSpace(request.Password) 
                ? user.PasswordHash : request.Password;

            await userManager.UpdateAsync(user);

            return new() { Id = user.Id };
        }
    }
}
