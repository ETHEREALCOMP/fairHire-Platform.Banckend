namespace FairHire.Domain;

public sealed class CandidateProfile
{
    public Guid UserId { get; set; }
    public string? Timezone { get; set; }
    public string? Level { get; set; } // "Junior/Middle/Senior" (simple for MVP)
    public List<string> Stacks { get; set; } = [];
    public string? About { get; set; }
    public bool OpenToWork { get; set; } = true;

    public User User { get; set; } = default!;
}
