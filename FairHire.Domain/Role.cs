using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class Role : IdentityRole<Guid>
{
    public Role() => Id = Guid.NewGuid();

    public string? Description { get; set; }
}
