namespace FairHire.Application.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsCompany { get; }
    bool IsCandidate { get; }
    bool IsAdmin { get; }
}
