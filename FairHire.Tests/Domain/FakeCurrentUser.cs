using FairHire.Application.CurrentUser;

namespace FairHire.Tests.Domain;

public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; init; } = Guid.NewGuid();
    public bool IsCompany { get; init; }
    public bool IsCandidate { get; init; }
    public bool IsAdmin { get; init; }
}
