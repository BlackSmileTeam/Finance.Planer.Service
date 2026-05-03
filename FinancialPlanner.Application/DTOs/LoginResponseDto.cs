namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a login response with authentication token.</para>
/// </summary>
public sealed class LoginResponseDto
{
    /// <summary>
    /// <para>Gets or sets the JWT token.</para>
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the token expiration time.</para>
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// <para>Gets or sets the user information.</para>
    /// </summary>
    public UserDto User { get; set; } = null!;
}

