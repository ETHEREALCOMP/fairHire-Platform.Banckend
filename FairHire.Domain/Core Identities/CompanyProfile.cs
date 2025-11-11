using FairHire.Domain.CompanyRubrics;
using FairHire.Domain.Infra;
using FairHire.Domain.Simulations;
using FairHire.Domain.TaskLibrary;

namespace FairHire.Domain;

public sealed class CompanyProfile
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? About { get; set; }
    public Guid? LogoFileId { get; set; }
    public bool IsVerified { get; set; }

    public User User { get; set; } = default!;
    public FileObject? LogoFile { get; set; }

    public List<TaskTemplate> TaskTemplates { get; set; } = new();
    public List<Simulation> Simulations { get; set; } = new();
    public List<Rubric> Rubrics { get; set; } = new();
}
