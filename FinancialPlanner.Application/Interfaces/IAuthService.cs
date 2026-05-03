using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Defines authentication service operations.</para>
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// <para>Registers a new user.</para>
    /// </summary>
    Task<LoginResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Logs in a user and returns a JWT token.</para>
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Validates a JWT token and returns the user ID.</para>
    /// </summary>
    Guid? ValidateToken(string token);

    /// <summary>
    /// <para>Updates a user's password.</para>
    /// </summary>
    Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default);
}

