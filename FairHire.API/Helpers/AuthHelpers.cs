using System.Security.Claims;

namespace FairHire.API.Helpers;

public static class AuthHelpers
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
        => Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
