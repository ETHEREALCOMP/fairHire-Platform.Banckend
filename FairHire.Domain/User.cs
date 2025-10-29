using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public required string Name { get; set; }
}
