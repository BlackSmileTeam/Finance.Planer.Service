namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Represents a user registration request.</para>
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// <para>Gets or sets the username.</para>
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the email address.</para>
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the password.</para>
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the full name.</para>
    /// </summary>
    public string? FullName { get; set; }
}

