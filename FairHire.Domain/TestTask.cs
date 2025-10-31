namespace FairHire.Domain;

public sealed class TestTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public string? Description { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public Guid CompanyId { get; set; }

    public Company? Company { get; set; }
}
