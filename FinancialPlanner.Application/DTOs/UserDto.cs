namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a user data transfer object.</para>
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// <para>Gets or sets the unique identifier.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the username.</para>
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the email address.</para>
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the full name.</para>
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the user is an administrator.</para>
    /// </summary>
    public bool IsAdministrator { get; set; }
}

