using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public required string Name { get; set; }

    public required string Password { get; set; }


    public IDictionary<string, string> SocialNetworks = new Dictionary<string, string>();
}
