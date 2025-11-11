namespace FairHire.Domain.CompanyRubrics;

public sealed class Rubric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; } // FK -> CompanyProfile.UserId
    public required string Name { get; set; }
    public required string CriteriaJson { get; set; } // [{key,label,weight,hint}]
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CompanyProfile Company { get; set; } = default!;
}
