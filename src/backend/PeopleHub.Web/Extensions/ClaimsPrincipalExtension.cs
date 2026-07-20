using System.Security.Claims;

namespace PeopleHub.Extensions;

public static class ClaimsPrincipalExtension
{
    extension(ClaimsPrincipal principal)
    {
        public long GetUserId() => long.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

        public bool HasUserId() => long.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out _);
    }
}
