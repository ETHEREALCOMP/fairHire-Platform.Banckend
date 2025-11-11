using FairHire.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class User : IdentityUser<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string Name { get; set; }
    public UserRole Role { get; set; } = UserRole.Candidate;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public CompanyProfile? CompanyProfile { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

}
