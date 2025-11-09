namespace FairHire.Domain;

public sealed class CompanyProfile
{
    public Guid UserId { get; set; }

    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }

    public User User { get; set; } = default!;
    public List<TestTask> CreatedTasks { get; set; } = [];
}
