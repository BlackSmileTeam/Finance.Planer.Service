namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Represents a user login request.</para>
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// <para>Gets or sets the username or email.</para>
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the password.</para>
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

