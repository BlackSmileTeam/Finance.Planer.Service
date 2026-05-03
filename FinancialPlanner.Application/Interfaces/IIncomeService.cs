using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for income cycles.</para>
/// </summary>
public interface IIncomeService
{
    /// <summary>
    /// <para>Returns income cycles filtered by year.</para>
    /// </summary>
    Task<IReadOnlyCollection<IncomeCycleDto>> GetByYearAsync(int year, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Creates a new income cycle.</para>
    /// </summary>
    Task<IncomeCycleDto> CreateAsync(CreateIncomeRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Updates an income cycle.</para>
    /// </summary>
    Task<IncomeCycleDto> UpdateAsync(Guid id, UpdateIncomeRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Deletes an income cycle.</para>
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Gets pending planned income that needs to be confirmed.</para>
    /// </summary>
    Task<IReadOnlyCollection<PendingPlannedTransactionDto>> GetPendingPlannedIncomeAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Confirms a planned income, marking it as actual.</para>
    /// </summary>
    Task ConfirmPlannedIncomeAsync(Guid incomeId, Guid userId, decimal? actualAmount, CancellationToken cancellationToken);
}

