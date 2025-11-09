using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public required string Name { get; set; }

    public CompanyProfile? CompanyProfile { get; set; }
    public DeveloperProfile? DeveloperProfile { get; set; }


    public List<TestTask> AssignedTasks { get; set; } = [];

}
