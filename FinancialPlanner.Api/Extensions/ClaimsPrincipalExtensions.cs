using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        // Accept multiple claim names because different JWT handlers/configs may map user id
        // to NameIdentifier, "sub", or "nameid".
        var claimCandidates = new[]
        {
            ClaimTypes.NameIdentifier,
            JwtRegisteredClaimNames.Sub,
            "nameid",
            "sub"
        };

        foreach (var claimType in claimCandidates)
        {
            var claim = principal.FindFirst(claimType);
            if (claim != null && Guid.TryParse(claim.Value, out var parsedUserId))
            {
                return parsedUserId;
            }
        }

        return null;
    }

    /// <summary>
    /// <para>Returns whether the principal has the Admin role (JWT claim).</para>
    /// </summary>
    public static bool IsAdministrator(this ClaimsPrincipal? principal) =>
        principal?.IsInRole("Admin") ?? false;
}

