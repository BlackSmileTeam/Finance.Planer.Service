using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Defines account service operations.</para>
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// <para>Returns all accounts for a user.</para>
    /// </summary>
    Task<IEnumerable<AccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Returns an account by identifier.</para>
    /// </summary>
    Task<AccountDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Creates a new account.</para>
    /// </summary>
    Task<AccountDto> CreateAsync(CreateAccountRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Updates an existing account.</para>
    /// </summary>
    Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Deletes an account.</para>
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}

