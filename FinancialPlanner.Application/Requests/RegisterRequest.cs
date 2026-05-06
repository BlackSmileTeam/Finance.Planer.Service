using System.ComponentModel.DataAnnotations;

namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Represents a user registration request.</para>
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// <para>Gets or sets the username.</para>
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the email address.</para>
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the password.</para>
    /// </summary>
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the full name.</para>
    /// </summary>
    public string? FullName { get; set; }
}

