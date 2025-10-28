using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }


    public IDictionary<string, string> SocialNetworks = new Dictionary<string, string>();
}
