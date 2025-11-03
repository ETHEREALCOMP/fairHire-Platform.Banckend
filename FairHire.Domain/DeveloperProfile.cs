namespace FairHire.Domain;

public sealed class DeveloperProfile
{
    public Guid UserId { get; set; }

    public List<string> Skills { get; set; } = [];

    public User User { get; set; } = default!;
}
