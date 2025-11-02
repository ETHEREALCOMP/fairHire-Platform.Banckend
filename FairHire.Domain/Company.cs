using Microsoft.AspNetCore.Identity;

namespace FairHire.Domain;

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Password { get; set; }

    public required string Name { get; set; }

    public required string UserEmail { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public required string UserRole { get; set; }

    public ICollection<TestTask> TestTasks { get; set; } = [];
}
