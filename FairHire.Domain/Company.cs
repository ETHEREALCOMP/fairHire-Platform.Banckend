namespace FairHire.Domain;

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public ICollection<TestTask> TestTasks { get; set; } = [];
}
