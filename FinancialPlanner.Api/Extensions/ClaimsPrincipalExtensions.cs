using System.Security.Claims;

namespace FinancialPlanner.Api.Extensions;

/// <summary>
/// <para>Extension methods for ClaimsPrincipal.</para>
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// <para>Gets the user ID from the claims principal.</para>
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}

