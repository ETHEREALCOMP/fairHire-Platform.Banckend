using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public required string Name { get; set; }

    public List<string>? Skills { get; set; } = [];

    public List<TestTask>? TestTasks { get; set; } = [];

}
