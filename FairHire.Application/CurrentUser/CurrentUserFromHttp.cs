using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FairHire.Application.CurrentUser;

public sealed class CurrentUserFromHttp : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserFromHttp(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var idStr = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsCompany => HasRole("Company");
    public bool IsCandidate => HasRole("Developer") || HasRole("Candidate");
    public bool IsAdmin => HasRole("Admin");

    private bool HasRole(string role)
        => Principal?.Claims.Any(c =>
               c.Type is ClaimTypes.Role or "role"
               && string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)) == true;
}
