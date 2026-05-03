namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to update a user's password.</para>
/// </summary>
public sealed class UpdatePasswordRequest
{
    /// <summary>
    /// <para>Gets or sets the current password.</para>
    /// </summary>
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// <para>Gets or sets the new password.</para>
    /// </summary>
    public required string NewPassword { get; init; }
}

